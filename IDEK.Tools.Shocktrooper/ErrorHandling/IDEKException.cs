using IDEK.Tools.Debugging;
using System;

namespace IDEK.Tools.ErrorHandling
{
    //TODO: move to IDEK.Tools.ErrorHandling
    public class IDEKException : Exception
    {
        public string errorTitle;
        public string errorDescription;
        public bool isFatal;

        #region Constructors
        public IDEKException() => CommonConstruct();
        public IDEKException(Exception innerException) : base(innerException.Message, innerException) => CommonConstruct();
        public IDEKException(string errorDesc, Exception innerException) : base(errorDesc, innerException)
        {
            CommonConstruct();
            errorDescription = errorDesc + "\nInner Exception:\n" + innerException.Message;
        }
        public IDEKException(string errorDesc)
        {
            CommonConstruct();
            errorDescription = errorDesc;
        }

        public IDEKException(string errorDesc, Exception innerException, bool isFatal) : this(errorDesc, innerException)
        {
            this.isFatal = isFatal;
        }

        //Protected by nature, but special case since it's all ctor logic
        protected virtual void CommonConstruct() => FillOutData();
        #endregion

        public override string Message => errorTitle + ": \n" + errorDescription;
        public override string ToString()
        {
            return $"{(isFatal ? "FATAL" : "NON-FATAL")} GAME ERROR | " +
                Message +
                "\n" + StackTrace;
        }

        public override string StackTrace => "Callstack: " + DHDebug.GetStackTrace(true) + "\nStack Trace: " + base.StackTrace;


        private void FillOutData()
        {
            errorTitle = "Generic Game Error";
            errorDescription = "Something went wrong with the current session. Press the button below to return to the title screen.";
            isFatal = false;
        }
    }
}
