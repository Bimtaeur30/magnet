using System.Collections.Generic;
using GameLib.EventChannelSystem;
using Magnet.Contracts;

namespace Magnet.Core.Events
{
    public static class InGameEvents
    {
        public static readonly BlockSelectedEvent BlockSelectedEvent = new();
        public static readonly BlockSelectionEndedEvent BlockSelectionEndedEvent = new();
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
    
    public sealed class BlockSelectionEndedEvent : GameEvent
    {
        public int SlotIndex { get; private set; }

        public BlockSelectionEndedEvent Init(int slotIndex)
        {
            SlotIndex = slotIndex;
            return this;
        }
    }

    public sealed class BlockCreatedEvent : GameEvent
    {
        public IReadOnlyList<IBlockSkinTarget> Blocks { get; private set; }
        public int SkinId { get; private set; }

        public BlockCreatedEvent Init(IReadOnlyList<IBlockSkinTarget> blocks, int skinId)
        {
            Blocks = blocks;
            SkinId = skinId;
            
            return this;
        }
    }
    
    public sealed class BlockDestroyedEvent : GameEvent
    {
        public IBlockSkinTarget Block { get; private set; }

        public BlockDestroyedEvent Init(IBlockSkinTarget block)
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
