using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemini.Modules.UndoRedo.UndoAction
{
    public class CompositeUndoAction : IUndoableAction
    {
        public CompositeUndoAction(string name, IEnumerable<IUndoableAction> combinedActions)
        {
            CombinedActions = combinedActions;
            Name = name;
        }

        public string Name { get; }

        public IEnumerable<IUndoableAction> CombinedActions { get; }

        public void Execute()
        {
            foreach (var subAction in CombinedActions)
                subAction.Execute();
        }

        public void Undo()
        {
            foreach (var subAction in CombinedActions)
                subAction.Undo();
        }
    }
}
