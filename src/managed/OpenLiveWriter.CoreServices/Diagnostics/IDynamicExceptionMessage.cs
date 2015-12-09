// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.CoreServices.Diagnostics
{
    public interface IDynamicExceptionMessage
    {
        bool AppliesTo(Type exceptionType, Exception e);
        ExceptionMessage GetMessage(Exception e);
    }

    public abstract class UnwindingDynamicExceptionMessage : IDynamicExceptionMessage
    {
        private readonly bool unwind;

        public UnwindingDynamicExceptionMessage(bool unwind)
        {
            this.unwind = unwind;
        }

        protected abstract bool AppliesToInternal(Exception e);

        public bool AppliesTo(Type exceptionType, Exception e)
        {
            if (unwind)
                return Unwind(e) != null;
            else
                return AppliesToInternal(e);
        }

        public abstract ExceptionMessage GetMessage(Exception e);

        protected Exception Unwind(Exception e)
        {
            while (e != null)
            {
                if (AppliesToInternal(e))
                    return e;
                e = e.InnerException;
            }
            return null;
        }
    }

    public class SimpleDynamicExceptionMessage : UnwindingDynamicExceptionMessage
    {
        protected readonly Type exceptionType;
        private readonly ExceptionMessage exceptionMessage;
        protected readonly bool unwind;

        public SimpleDynamicExceptionMessage(Type exceptionType, ExceptionMessage exceptionMessage, bool recurseInnerExceptions) : base(recurseInnerExceptions)
        {
            this.exceptionType = exceptionType;
            this.exceptionMessage = exceptionMessage;
        }

        protected override bool AppliesToInternal(Exception e)
        {
            return this.exceptionType.IsInstanceOfType(e);
        }

        public override ExceptionMessage GetMessage(Exception e)
        {
            return exceptionMessage;
        }

    }

    public abstract class ErrorCodeDynamicExceptionMessage : UnwindingDynamicExceptionMessage
    {
        private readonly Type exceptionType;
        private readonly int errorCode;
        private readonly ExceptionMessage exceptionMessage;

        public ErrorCodeDynamicExceptionMessage(Type exceptionType, int errorCode, ExceptionMessage exceptionMessage) : base(true)
        {
            this.exceptionType = exceptionType;
            this.errorCode = errorCode;
            this.exceptionMessage = exceptionMessage;
        }

        protected abstract int GetErrorCode(Exception e);

        protected override bool AppliesToInternal(Exception e)
        {
            return (exceptionType.IsInstanceOfType(e) && errorCode == GetErrorCode(e));
        }

        public override ExceptionMessage GetMessage(Exception e)
        {
            return exceptionMessage;
        }
    }

    public class COMErrorCodeDynamicExceptionMessage : ErrorCodeDynamicExceptionMessage
    {
        public COMErrorCodeDynamicExceptionMessage(int errorCode, ExceptionMessage exceptionMessage) : base(typeof(COMException), errorCode, exceptionMessage)
        {
        }

        protected override int GetErrorCode(Exception e)
        {
            return ((COMException)e).ErrorCode;
        }
    }

    public class Win32ErrorCodeDynamicExceptionMessage : ErrorCodeDynamicExceptionMessage
    {
        public Win32ErrorCodeDynamicExceptionMessage(int errorCode, ExceptionMessage exceptionMessage) : base(typeof(Win32Exception), errorCode, exceptionMessage)
        {
        }

        protected override int GetErrorCode(Exception e)
        {
            return ((Win32Exception)e).ErrorCode;
        }
    }

}
