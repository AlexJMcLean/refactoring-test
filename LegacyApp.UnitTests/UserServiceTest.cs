using System;
using AutoFixture;
using FluentAssertions;
using LegacyApp;
using LegacyApp.DataAccess;
using LegacyApp.Models;
using LegacyApp.Repository;
using LegacyApp.Services;
using NSubstitute;
using Xunit;

namespace LecagyApp.UnitTests
{
    public class UserServiceTests
    {
        private readonly UserService _sut; // System under test
        private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        private readonly IClientRepository _clientRepository = Substitute.For<IClientRepository>();
        private readonly IUserDataAccess _userDataAccess = Substitute.For<IUserDataAccess>();
        private readonly IUserCreditService _userCreditService = Substitute.For<IUserCreditService>();
        private readonly IFixture _fixture = new Fixture();

        public UserServiceTests()
        {
            _sut = new UserService(_dateTimeProvider, _clientRepository, _userCreditService, _userDataAccess);

        }

        [Fact]
        public void AddUser_ShouldCreateUser_WhenAllParametersAreValid()
        {
            // Arrange
            var clientId = 1;
            var clientFirstName = "Alex";
            var clientLastName = "McLean";
            var dateOfBirth = new DateTime(1990, 1, 1);
            var client = _fixture.Build<Client>()
                .With(client => client.Id, clientId)
                .Create();
            _dateTimeProvider.DateTimeNow.Returns(new DateTime(2024, 2, 1));
            _clientRepository.GetById(client.Id).Returns(client);
            _userCreditService.GetCreditLimit(clientFirstName, clientLastName, dateOfBirth).Returns(600);


            // Act
            var result = _sut.AddUser(clientFirstName, clientLastName, "alex@test.com", dateOfBirth, client.Id);


            // Assert
            result.Should().Be(true);
            _userDataAccess.Received(1).AddUser(Arg.Any<User>());

        }

        [Theory]
        [InlineData("", "McLean", "alex@test.com", 1900)]
        [InlineData("Alex", "", "alex@test.com", 1900)]
        [InlineData("Alex", "McLean", "@", 1900)]
        [InlineData("Alex", "McLean", "alex@test.com", 2020)]
        public void AddUser_ShouldFail_WhenParametersAreInvalid(
            string firstName, string lastName, string email, int yearOfBirth)
        {
            var clientId = 1;
            var dateOfBirth = new DateTime(yearOfBirth, 1, 1);
            var client = _fixture.Build<Client>()
                .With(client => client.Id, clientId)
                .Create();
            _dateTimeProvider.DateTimeNow.Returns(new DateTime(2024, 2, 1));
            _clientRepository.GetById(Arg.Is(client.Id)).Returns(client);
            _userCreditService.GetCreditLimit(Arg.Is(firstName), Arg.Is(lastName), Arg.Is(dateOfBirth)).Returns(600);


            // Act
            var result = _sut.AddUser(firstName, lastName, email, dateOfBirth, client.Id);


            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("VeryImportantClient", false, 0, 0)]
        [InlineData("ImportantClient", true, 600, 1200)]
        [InlineData("JoeBlogs", true, 600, 600)]
        public void AddUser_ShouldCreateUserWithCorrectCreditLimit_WhenNameIsExpected(
            string clientName, bool hasCreditLimit, int initialCreditLimit, int finalCreditLimit)
        {
            var clientId = 1;
            var firstName = "Joe";
            var lastName = "bloggs";
            var email = "test@test.com";
            var dateOfBirth = new DateTime(1990, 1, 1);

            var client = _fixture.Build<Client>()
                .With(client => client.Id, clientId)
                .With(client => client.Name, clientName)
                .Create();

            _dateTimeProvider.DateTimeNow.Returns(new DateTime(2024, 2, 1));
            _clientRepository.GetById(Arg.Is(client.Id)).Returns(client);
            _userCreditService.GetCreditLimit(Arg.Is(firstName), Arg.Is(lastName), Arg.Is(dateOfBirth)).Returns(initialCreditLimit);


            // Act
            var result = _sut.AddUser(firstName, lastName, email, dateOfBirth, client.Id);


            // Assert
            result.Should().BeTrue();
            _userDataAccess.Received().AddUser(Arg.Is<User>(user => user.CreditLimit == finalCreditLimit && user.HasCreditLimit == hasCreditLimit));
        }

        [Fact]
        public void AddUser_ShouldNotCreateUser_WhenCreditLimitIsLessThan500()
        {
            var clientId = 1;
            var firstName = "Joe";
            var lastName = "bloggs";
            var email = "test@test.com";
            var dateOfBirth = new DateTime(1990, 1, 1);

            var client = _fixture.Build<Client>()
                .With(client => client.Id, clientId)
                .Create();

            _dateTimeProvider.DateTimeNow.Returns(new DateTime(2024, 2, 1));
            _clientRepository.GetById(Arg.Is(client.Id)).Returns(client);
            _userCreditService.GetCreditLimit(Arg.Is(firstName), Arg.Is(lastName), Arg.Is(dateOfBirth)).Returns(300);


            // Act
            var result = _sut.AddUser(firstName, lastName, email, dateOfBirth, client.Id);


            // Assert
            result.Should().BeFalse();
        }
    }
}