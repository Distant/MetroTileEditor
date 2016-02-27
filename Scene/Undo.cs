using UnityEngine;
using System.Collections.Generic;
using MetroTileEditor;
using System;
namespace MetroTileEditor
{
    public class Undo
    {

        public class UndoCommand
        {
            public Action Undo;
            public Action Redo;

            public UndoCommand(Action Undo, Action Redo)
            {
                this.Undo = Undo;
                this.Redo = Redo;
            }


        }

        private readonly int MAX_UNDO = 50;
        private Stack<UndoCommand> undoStack;
        private Stack<UndoCommand> redoStack;

        public Undo()
        {
            undoStack = new Stack<UndoCommand>();
            redoStack = new Stack<UndoCommand>();
        }

        public void RegisterUndo(Undo.UndoCommand command)
        {
            undoStack.Push(command);
            redoStack.Clear();
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
    }
}