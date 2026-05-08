using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;

namespace DoDoEng.Game.C1_G03
{
    public class UndoMGR
    {
        // Properties
        public bool CanUndo => commands.Count() > 0;

        // Methods
        public void AddCommand(Command command)
        {
            LOG.Function(this);

            commands.Push(command);
        }
        public void AddCheckPoint()
        {
            LOG.Function(this);

            AddCommand(Command.OfCheckPoint());
        }
        public Command[] Undo()
        {
            LOG.Function(this);

            if (!CanUndo) return null;

            var list = new List<Command>();

            commands.Pop(); // remove last checkPoint
            while (commands.Count() > 0 &&
                   commands.Peek().Type != CommandType.CheckPoint)
                list.Add(commands.Pop());

            return list.ToArray();
        }
        public void Clear()
        {
            LOG.Function(this);

            commands.Clear();
        }



        // Fields
        private Stack<Command> commands = new Stack<Command>();
    }



    public enum CommandType { Add, Remove, CheckPoint }
    public class Command
    {
        // Properties
        public CommandType Type { get; private set; }
        public Cell Cell { get; private set; }
        public RoadType RoadType { get; private set; }

        // Methods
        public static Command OfAdd(Cell cell, Road road) => new Command(CommandType.Add, cell, road);
        public static Command OfRemove(Cell cell, Road road) => new Command(CommandType.Remove, cell, road);
        public static Command OfCheckPoint() => theCheckPoint;



        // Overrides
        public override string ToString()
        {
            return $"Command[{Type} | {Cell} | {RoadType}]";
        }



        // Fields
        private static Command theCheckPoint = new Command(CommandType.CheckPoint);

        // Functions : ctor.
        private Command(CommandType type)
        {
            Type = type;
        }
        private Command(CommandType type, Cell cell, Road road)
        {
            Type = type;
            Cell = cell;
            RoadType = road.RoadType;
        }
    }
}