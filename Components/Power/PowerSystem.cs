using System;
using CS.Components.Grid;
using CS.SlimeFactory;
using Godot;

namespace CS.Components.Power;

public partial class PowerSystem : NodeSystem
{
    [InjectDependency] private readonly GridSystem _gridSystem = null!;
    
    public override void _Ready()
    {
        base._Ready();

        // _nodeManager.SignalBus.PowerGeneratedSignal += OnPowerGenerated;
        _nodeManager.SignalBus.NodeAnchoredToGridSignal += OnNodeAnchored;
        _nodeManager.SignalBus.NodeUnanchoredFromGridSignal += OnNodeUnanchored;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        
        ProcessPowerGenerators(delta);
        ProcessPowerTransmitters(delta);
        ProcessPowerDistributors(delta);
        ProcessPowerCustomers(delta);
    }

    #region Signal Handlers
    private void OnNodeAnchored(RigidBody2D node, ref NodeAnchoredToGridSignal args)
    {
        if (_nodeManager.TryGetComponent<PowerTransmissionComponent>(node, out var powerTransmissionComponent))
        {
            ConnectTransmitter((node, powerTransmissionComponent));
            return;
        }

        if (_nodeManager.TryGetComponent<PowerDistributorComponent>(node, out var powerDistributorComponent))
        {
            ConnectDistributor((node, powerDistributorComponent));
            return;
        }

        if (_nodeManager.TryGetComponent<PowerCustomerComponent>(node, out var powerCustomerComponent))
        {
            ConnectCustomer((node, powerCustomerComponent));
            return;
        }
    }

    private void OnNodeUnanchored(RigidBody2D node, ref NodeUnanchoredFromGridSignal args)
    {
        if (_nodeManager.TryGetComponent<PowerTransmissionComponent>(node, out var powerTransmissionComponent))
        {
            powerTransmissionComponent.ConnectedGenerators.Clear();
            return;
        }

        if (_nodeManager.TryGetComponent<PowerDistributorComponent>(node, out var powerDistributorComponent))
        {
            powerDistributorComponent.ConnectedTransmitters.Clear();
            return;
        }

        if (_nodeManager.TryGetComponent<PowerCustomerComponent>(node, out var powerCustomerComponent))
        {
            powerCustomerComponent.ConnectedDistributors.Clear();
            return;
        }
    }
    #endregion

    #region Processes
    private void ProcessPowerGenerators(double delta)
    {
        _nodeManager.NodeQuery<PowerGeneratorComponent>(out var generators);
        foreach (var (node, comp) in generators)
        {
            comp.PowerGenerated = comp.PowerRate * (float)delta;
        }
    }

    private void ProcessPowerTransmitters(double delta)
    {
        _nodeManager.NodeQuery<PowerTransmissionComponent>(out var transmitters);
        foreach (var (node, comp) in transmitters)
        {
            var maxPossibleTransmitted = comp.TransmissionRate * (float)delta;
            comp.PowerToTransmit = 0;
            
            foreach (var generator in comp.ConnectedGenerators)
            {
                if (!_nodeManager.TryGetComponent<PowerGeneratorComponent>(generator, out var generatorComp))
                    continue;

                if (maxPossibleTransmitted <= comp.PowerToTransmit)
                    continue;

                var availablePower = generatorComp.PowerGenerated;
                var desiredPower = maxPossibleTransmitted - comp.PowerToTransmit;
                var powerTaken = float.Min(desiredPower, availablePower);
                generatorComp.PowerGenerated -= powerTaken;
                comp.PowerToTransmit += powerTaken;
            }
        }
    }

    private void ProcessPowerDistributors(double delta)
    {
        _nodeManager.NodeQuery<PowerDistributorComponent>(out var distributors);
        foreach (var (node, comp) in distributors)
        {
            var maxPossibleDistributed = comp.DistributionRate * (float)delta;
            comp.PowerToDistribute = 0;
            
            foreach (var transmitter in comp.ConnectedTransmitters)
            {
                if (!_nodeManager.TryGetComponent<PowerTransmissionComponent>(transmitter, out var transmitterComp))
                    continue;

                if (maxPossibleDistributed <= comp.PowerToDistribute)
                    continue;

                var availablePower = transmitterComp.PowerToTransmit;
                var desiredPower = maxPossibleDistributed - comp.PowerToDistribute;
                var powerTaken = float.Min(desiredPower, availablePower);
                transmitterComp.PowerToTransmit -= powerTaken;
                comp.PowerToDistribute += powerTaken;
            }
        }
    }

    private void ProcessPowerCustomers(double delta)
    {
        _nodeManager.NodeQuery<PowerCustomerComponent>(out var customers);
        foreach (var (node, comp) in customers)
        {
            var maxPossibleConsumed = comp.ConsumptionRate * (float)delta;
            comp.PowerConsumed = 0;

            foreach (var distributor in comp.ConnectedDistributors)
            {
                if (!_nodeManager.TryGetComponent<PowerDistributorComponent>(distributor, out var distributorComp))
                    continue;

                if (maxPossibleConsumed <= comp.PowerConsumed)
                    continue;

                var availablePower = distributorComp.PowerToDistribute;
                var desiredPower = maxPossibleConsumed - comp.PowerConsumed;
                var powerTaken = float.Min(desiredPower, availablePower);
                distributorComp.PowerToDistribute -=  powerTaken;
                comp.PowerConsumed += powerTaken;
            }
            
            comp.IsSufficientlyPowered = Math.Abs(maxPossibleConsumed - comp.PowerConsumed) < 0.00001f;
        }
    }
    #endregion

    #region Connections
    private void ConnectTransmitter(Node<PowerTransmissionComponent> node)
    {
        _nodeManager.NodeQuery<PowerGeneratorComponent>(out var generators);
        foreach (var (generator, generatorComp) in generators)
        {
            if (!_gridSystem.TryGetDistance(node, generator, out var distance))
                continue;

            if (generatorComp.Range < distance)
                continue;
            
            node.Comp.ConnectedGenerators.Add(generator);
        }
    }

    private void ConnectDistributor(Node<PowerDistributorComponent> node)
    {
        _nodeManager.NodeQuery<PowerTransmissionComponent>(out var transmitters);
        foreach (var (transmitter, transmissionComp) in transmitters)
        {
            if (!_gridSystem.TryGetDistance(node, transmitter, out var distance))
                continue;

            if (transmissionComp.Range < distance)
                continue;
            
            node.Comp.ConnectedTransmitters.Add(transmitter);
        }
    }

    private void ConnectCustomer(Node<PowerCustomerComponent> node)
    {
        _nodeManager.NodeQuery<PowerDistributorComponent>(out var distributors);
        foreach (var (distributor, distributorComp) in distributors)
        {
            if (!_gridSystem.TryGetDistance(node, distributor, out var distance))
                continue;

            if (distributorComp.Range < distance)
                continue;
            
            node.Comp.ConnectedDistributors.Add(distributor);
        }
    }
    #endregion

}