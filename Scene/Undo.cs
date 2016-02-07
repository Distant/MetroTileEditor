using UnityEngine;
using System.Collections.Generic;
using MetroTileEditor;
using System;

public class Undo : ISerializationCallbackReceiver
{

    public interface UndoCommand
    {
        void Undo();
        void Redo();
    }

    private Stack<UndoCommand> undoStack;
    private Stack<UndoCommand> redoStack;

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {

    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {

    }

    public Undo()
    {
        undoStack = new Stack<UndoCommand>();
        redoStack = new Stack<UndoCommand>();
    }

    public void PerformUndo()
    {
        if (undoStack.Count > 0)
        {
            UndoCommand command = undoStack.Pop();
            command.Undo();
            redoStack.Push(command);
        }
    }

    public void PerformRedo()
    {
        if (redoStack.Count > 0)
        {
            UndoCommand command = redoStack.Pop();
            command.Redo();
            undoStack.Push(command);
        }
    }

    public void RegisterUndo(UndoCommand command)
    {
        undoStack.Push(command);
        redoStack.Clear();
    }

    public class AddCommand : UndoCommand
    {
        int x, y, z;
        string blockType;
        MapObject obj;

        public AddCommand(int x, int y, int z, string blockType, MapObject obj)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.blockType = blockType;
            this.obj = obj;
        }

        public void Redo()
        {
            obj._AddBlock(x, y, z, blockType);
        }

        public void Undo()
        {
            obj.blocks.DeleteBlock(x, y, z);
            obj.DrawBlocks();
        }
    }

    public class DeleteCommand : UndoCommand
    {
        int x, y, z;
        string blockType;
        MapObject obj;
        BlockData data;

        public DeleteCommand(int x, int y, int z, BlockData data, MapObject obj)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.data = data;
            this.obj = obj;
        }

        public void Redo()
        {
            obj.blocks.DeleteBlock(x, y, z);
            obj.DrawBlocks();
        }

        public void Undo()
        {
            obj._AddBlock(x, y, z, data);
        }
    }
}
