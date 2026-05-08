using System;

namespace DoDoEng.Common
{
    public class NoVersionInfoException : Exception
    {
        public NoVersionInfoException() { }
        public NoVersionInfoException(string msg) : base(msg) { }
    }

    public class NoListException : Exception
    {
        public NoListException() { }
        public NoListException(string msg) : base(msg) { }
    }

    public class NoDataException : Exception
    {
        public NoDataException() { }
        public NoDataException(string msg) : base(msg) { }
    }

    public class NoPageException : Exception
    {
        public NoPageException() { }
        public NoPageException(string msg) : base(msg) { }
    }
    public class NoSequenceException : Exception
    {
        public NoSequenceException() { }
        public NoSequenceException(string msg) : base(msg) { }
    }

    public class NoRecordException : Exception
    {
        public NoRecordException() { }
        public NoRecordException(string msg) : base(msg) { }
    }
}

