using System.Collections.Generic;
using GameLib.EventChannelSystem;
using JTH.Scripts.Domain.Spawn;
using JTH.Scripts.Presentation;
using Magnet.Contracts;
using UnityEngine;

namespace JTH.Scripts.Events
{
    public static class InGameEvents
    {
        public static readonly BlockSelectedEvent BlockSelectedEvent = new();
        public static readonly ShapeBlockCreatedEvent ShapeBlockCreatedEvent = new();
    }
        
    public sealed class BlockSelectedEvent : GameEvent
    {
        public int SlotIndex { get; private set; }
        public ShapeBlockData BlockData { get; private set; }

        public BlockSelectedEvent Init(int slotIndex, ShapeBlockData spawnData)
        {
            SlotIndex = slotIndex;
            BlockData = spawnData;
            return this;
        }
    }
    
    public sealed class ShapeBlockCreatedEvent : GameEvent
    {
        public IReadOnlyList<Block> Blocks { get; private set; }
        public int SkinId { get; private set; }

        public ShapeBlockCreatedEvent Init(IReadOnlyList<Block> blocks, int skinId)
        {
            Blocks = blocks;
            SkinId = skinId;
            
            return this;
        }
    }
}