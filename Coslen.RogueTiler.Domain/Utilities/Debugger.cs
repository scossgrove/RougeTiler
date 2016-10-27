using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Coslen.RogueTiler.Domain.Engine;
using Coslen.RogueTiler.Domain.Engine.Entities;
using Coslen.RogueTiler.Domain.Utilities.Configuration;

namespace Coslen.RogueTiler.Domain.Utilities
{
    public enum LogLevel
    {
        None = 0,
        Debug,
        Info,
        Error
    }

    public class Debugger
    {
        private static Debugger instance;
        private readonly Dictionary<Monster, _MonsterLog> _monsters;

        private readonly bool enabled;
        private readonly LogLevel minLevel;

        private Debugger()
        {
            enabled = true;
            _monsters = new Dictionary<Monster, _MonsterLog>();
            
            var level = RogueTilerSettings.Settings.DebugLevel;

            switch (level)
            {
                case "Debug":
                    {
                        minLevel = LogLevel.Debug;
                        break;
                    }
                case "Info":
                    {
                        minLevel = LogLevel.Info;
                        break;
                    }
                default:
                    {
                        minLevel = LogLevel.Error;
                        break;
                    }
            }


        }

        //Static instance which allows it to be accessed by any other script.
        public static Debugger Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Debugger();
                }

                return instance;
            }
        }

        public void addMonster(Monster monster)
        {
            if (!enabled)
            {
                return;
            }
            _monsters[monster] = new _MonsterLog(monster);

            var monsterLog = _monsters[monster];

            LogToDisk(monsterLog.BuildMessage("Adding new monster"));
        }

        public void logMonster(Monster monster, string message)
        {
            if (!enabled)
            {
                return;
            }
            var monsterLog = _monsters[monster];
            monsterLog.add(message);

            LogToDisk(monsterLog.BuildMessage(message));
        }

        public void removeMonster(Monster monster)
        {
            if (!enabled)
            {
                return;
            }

            var monsterLog = _monsters[monster];
            LogToDisk(monsterLog.BuildMessage("Removing existing monster"));

            _monsters.Remove(monster);
        }

        public void Info(string message)
        {
            LogToDisk(message, LogLevel.Info);
        }

        public void LogToDisk(string message, LogLevel level = LogLevel.Debug)
        {
            var path = GamePathUtilities.GetDebugFilePath();
            // This text is added only once to the file.
            if (!File.Exists(path))
            {
                // Create a file to write to.
                using (var sw = File.CreateText(path))
                {
                    sw.WriteLine("{0} - {1}", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff"), "starting log file");
                }
            }

            if ((int)level >= (int)minLevel)
            {
                // Create a file to write to.
                using (var sw = File.AppendText(path))
                {
                    sw.WriteLine("{0} - {1}", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff"), message);
                }
            }
        }

        public void addStage(Stage stage)
        {
            var mazeDebug = string.Empty;

            mazeDebug += "\r\n";
            mazeDebug += "\r\n--------------------------------------------------------------";
            mazeDebug += "\r\n-- " + "Stage Data";
            mazeDebug += "\r\n--------------------------------------------------------------";
            mazeDebug += "\r\n";

            var appearenceMatrix = stage.Appearances;
            
            var matrixWidth = appearenceMatrix.GetUpperBound(0) + 1;
            var matrixHeight = appearenceMatrix.GetUpperBound(1) + 1;

            for (var rowIndex = 0; rowIndex < matrixHeight; rowIndex++)
            {
                for (var columnIndex = 0; columnIndex < matrixWidth; columnIndex++)
                {
                    var appearance = appearenceMatrix[columnIndex, rowIndex];

                    mazeDebug += appearance.Glyph;
                }

                mazeDebug += "\r\n";
            }

            LogToDisk(mazeDebug);
        }
    }

    public class _MonsterLog
    {
        public Queue<string> log = new Queue<string>();
        public Monster monster;

        public _MonsterLog(Monster monster)
        {
            this.monster = monster;
        }

        public void add(string logItem)
        {
            log.Enqueue(logItem);
            if (log.Count > 10)
            {
                log.Dequeue();
            }
        }

        public string BuildMessage(string message)
        {
            return message + " :: " + monster.Breed.Name + "(" + monster.Id + ")";
        }

        public override string ToString()
        {
            var buffer = new StringBuilder();

            buffer.Append(monster.Breed.Name);
            buffer.Append(" health: ${monster.health.current}/${monster.health.max}");
            while (log.Count != 0)
            {
                var first = log.Dequeue();
                buffer.Append(first + "\n");
            }
            return buffer.ToString();
        }
    }
}