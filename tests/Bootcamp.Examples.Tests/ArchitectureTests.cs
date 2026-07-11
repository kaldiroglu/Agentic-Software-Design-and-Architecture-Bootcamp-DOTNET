using Xunit;
using LayeredPersistence = dev.kaldiroglu.bootcamp.layered.persistence;
using LayeredBusiness = dev.kaldiroglu.bootcamp.layered.business;
using LayeredPresentation = dev.kaldiroglu.bootcamp.layered.presentation;
using HexDomain = dev.kaldiroglu.bootcamp.hexagonal.domain;
using HexAdapter = dev.kaldiroglu.bootcamp.hexagonal.adapter;

namespace Bootcamp.Examples.Tests;

public class ArchitectureTests
{
    [Fact]
    public void Layered_RequestFlowsDownAndPersists()
    {
        var repo = new LayeredPersistence.InMemoryOrderRepository();
        var service = new LayeredBusiness.OrderService(repo);
        var controller = new LayeredPresentation.OrderController(service);
        Assert.Equal("201 Created", controller.Place("2x coffee"));
        Assert.Equal("400 Bad Request", controller.Place("  "));
    }

    [Fact]
    public void Hexagonal_AdapterPlugsIntoTheDomainPort()
    {
        var service = new HexDomain.OrderService(new HexAdapter.InMemoryOrderRepository());
        service.Place("2x tea");
        service.Place("1x cake");
        Assert.Equal(2, service.PlacedCount());
    }
}
