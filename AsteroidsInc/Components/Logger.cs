using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AsteroidsInc.Components
{
    public static class Logger //simple logging class used for writing lines to a file
    {
        static FileStream stream = new FileStream(filePath, FileMode.Append, FileAccess.Write);
        const string filePath = @"log.txt";

        public static void WriteLog(string message) //simple string writer
        {
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.WriteLine(message);
            }
            resetStream();
        }

        public static void WriteLog(params string[] messages) //string params writer
        {
            using (StreamWriter writer = new StreamWriter(stream))
            {
                foreach (string message in messages)
                    writer.WriteLine(message);
            }
            resetStream();
        }

        public static void WriteLog(params object[] messages) //object.ToString writer
        {
            using (StreamWriter writer = new StreamWriter(stream))
            {
                foreach (object message in messages)
                    writer.WriteLine(message.ToString());
            }
            resetStream();
        }

        static void resetStream() //workaround for stream closing prematurely
        {
            stream = new FileStream(filePath, FileMode.Append, FileAccess.Write);
        }
    }
}
