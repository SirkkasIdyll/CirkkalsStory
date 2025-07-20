﻿using System.Diagnostics.CodeAnalysis;
using CS.Components.CombatManager;
using CS.Components.Mob;
using CS.SlimeFactory;
using Godot;
using Godot.Collections;

namespace CS.Components.Magic;

public partial class MagicSystem : NodeSystem
{
    private Dictionary<string, Node> _spellDictionary = [];
    
    public override void _Ready()
    {
        base._Ready();

        _nodeManager.SignalBus.PreviewActionSignal += OnPreviewAction;
        _nodeManager.SignalBus.UseActionSignal += OnUseAction;
        _nodeManager.SignalBus.CheckSpellCastableSignal += OnCheckSpellCastable;
        
        LoadDictionary();
    }
    
    private void OnUseAction(Node<MobComponent> node, ref UseActionSignal args)
    {
        if (!_nodeManager.TryGetComponent<SpellComponent>(args.Action, out var spellComponent))
            return;
        
        CastSpell(node, (args.Action, spellComponent), false);
    }

    private void OnPreviewAction(Node<MobComponent> node, ref PreviewActionSignal args)
    {
        if (!_nodeManager.TryGetComponent<SpellComponent>(args.Action, out var spellComponent))
            return;
        
        CastSpell(node, (args.Action, spellComponent), true);
    }
    
    private void OnCheckSpellCastable(Node<SpellComponent> node, ref CheckSpellCastableSignal args)
    {
        if (!_nodeManager.TryGetComponent<ManaCostComponent>(node, out var manaCostComponent))
            return;
        
        if (!_nodeManager.TryGetComponent<ManaComponent>(args.Spellcaster, out var spellcasterManaComponent))
            return;

        if (manaCostComponent.ManaCost >= spellcasterManaComponent.Mana)
            return;
        
        args.Castable = true;
    }

    private void CastSpell(Node<MobComponent> node, Node<SpellComponent> spell, bool preview)
    {
        if (!_nodeManager.TryGetComponent<ManaComponent>(node, out var spellcasterManaComponent))
            return;
        
        if (!_nodeManager.TryGetComponent<ManaCostComponent>(spell, out var manaCostComponent))
            return;

        if (preview)
        {
            var previewSignal = new PreviewManaAlteredSignal(-manaCostComponent.ManaCost);
            _nodeManager.SignalBus.EmitPreviewManaAlteredSignal((node, spellcasterManaComponent), ref previewSignal);
            return;
        }
        
        spellcasterManaComponent.Mana -= manaCostComponent.ManaCost;
        var signal = new ManaAlteredSignal();
        _nodeManager.SignalBus.EmitManaAlteredSignal((node, spellcasterManaComponent), ref signal);
    }
    
    /// <summary>
    /// Fetches an array of the known spells
    /// </summary>
    /// <param name="node">A mob</param>
    /// <param name="spells">Spells from the <see cref="MobComponent"/></param>
    public void GetKnownSpells(Node node, out Array<string> spells)
    {
        spells = [];

        if (!_nodeManager.TryGetComponent<MobComponent>(node, out var mobComponent))
            return;

        spells = mobComponent.Spells;
    }
    
    /// <summary>
    /// Loads all spells into the spell repository.
    /// Skill details can be retrieved from the spell repository.
    /// </summary>
    private void LoadDictionary()
    {
        foreach (var node in _nodeManager.NodeDictionary.Values)
        {
            if (!_nodeManager.TryGetComponent<SpellComponent>(node, out var spellComponent))
                continue;
            
            _spellDictionary.Add(node.Name, node);
        }
    }
    
    /// <summary>
    /// Attempts to return the spell node if it exists in the spell repository
    /// </summary>
    /// <param name="name">The name of the spell to retrieve</param>
    /// <param name="spell">The node containing the spell and all its child components</param>
    /// <returns>True if spell found, false if spell not found</returns>
    public bool TryGetSpell(string name, [NotNullWhen(true)] out Node? spell)
    {
        return _spellDictionary.TryGetValue(name, out spell);
    }

    /// <summary>
    /// Checks if the spell exists in the repository
    /// </summary>
    /// <param name="name">The name of the spell</param>
    /// <returns>True if spell found, false if spell not found</returns>
    public bool SpellExists(string name)
    {
        return _spellDictionary.ContainsKey(name);
    }
}