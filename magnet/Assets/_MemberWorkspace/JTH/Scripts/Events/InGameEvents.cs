using System.Collections.Generic;
using GameLib.EventChannelSystem;
using JTH.Scripts.Domain.Placement;
using JTH.Scripts.Presentation;
using Magnet.Contracts;

namespace JTH.Scripts.Events
{
    public static class InGameEvents
    {
        public static readonly BlockSelectedEvent BlockSelectedEvent = new();
        public static readonly BlockCreatedEvent BlockCreatedEvent = new();
        public static readonly BlockDestroyedEvent BlockDestroyedEvent = new();
        public static readonly BlockPlacedEvent BlockPlacedEvent = new();
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
    
    public sealed class BlockCreatedEvent : GameEvent
    {
        public IReadOnlyList<Block> Blocks { get; private set; }
        public int SkinId { get; private set; }

        public BlockCreatedEvent Init(IReadOnlyList<Block> blocks, int skinId)
        {
            Blocks = blocks;
            SkinId = skinId;
            
            return this;
        }
    }
    
    public sealed class BlockDestroyedEvent : GameEvent
    {
        public Block Block { get; private set; }

        public BlockDestroyedEvent Init(Block block)
        {
            Block = block;
            
            return this;
        }
    }
    
    public sealed class BlockPlacedEvent : GameEvent
    {
        public PlacementResult PlacementResult { get; private set; }
        
        public BlockPlacedEvent Init(PlacementResult placementResult)
        {
            PlacementResult = placementResult;

            return this;
        }
    }
}