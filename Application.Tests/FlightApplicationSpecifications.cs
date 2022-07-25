using Xunit;
using System.Collections.Generic;
using System;
using FluentAssertions;
using Data;
using Domain;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Application.Tests
{
    public class FlightApplicationSpecifications
    {
        readonly Entities entities = new Entities(new DbContextOptionsBuilder<Entities>()
                .UseInMemoryDatabase("Flights")
                .Options);

        readonly BookingService bookingService;

        public FlightApplicationSpecifications()
        {
            bookingService = new BookingService(entities: entities);
        }


        [Theory]
        [InlineData("m@m.com", 2)]
        [InlineData("a@a.com", 2)]
        public void Remembers_bookings(string passengerEmail, int numberOfSeats)
        {           
            var flight = new Flight(3);

            entities.Flights.Add(flight);

            bookingService.Book(new BookDto(
                flightId: flight.Id, passengerEmail, numberOfSeats));

            bookingService.FindBookings(flight.Id).Should().ContainEquivalentOf(
                new BookingRm(passengerEmail, numberOfSeats)
                );
        }

        [Theory]
        [InlineData(3)]
        [InlineData(10)]
        public void Frees_up_seats_after_booking(int initialCapacity)
        {
            // Given
            var flight = new Flight(initialCapacity);
            entities.Flights.Add(flight);

            bookingService.Book(new BookDto(flightId: flight.Id,
                passengerEmail: "m@m.com",
                numberOfSeats: 2));

            // When
            bookingService.CancelBooking(                
                new CancelBookingDto(
                    flightId: flight.Id,
                    passengerEmail: "m@m.com",
                    numberOfSeats: 2)                
                );

            // Then
            bookingService.GetRemainingNumberOfSeatsFor(flight.Id)
                .Should().Be(initialCapacity);
        }
    }

}