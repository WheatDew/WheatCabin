using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.RTSL.Interface
{
    public class Error
    {
        public const int OK = 0;
        public const int E_Exception = 1;
        public const int E_NotFound = 2;
        public const int E_AlreadyExist = 3;
        public const int E_InvalidOperation = 4;
        public const int E_Failed = 5;

        public static readonly Error NoError = new Error(OK);

        public int ErrorCode;

        public string ErrorText;

        public bool HasError
        {
            get { return ErrorCode != 0; }
        }

        public Error()
        {
            ErrorCode = OK;
        }

        public Error(int errorCode)
        {
            ErrorCode = errorCode;
        }

        public override string ToString()
        {
            return string.Format("Error({0}): {1}", ErrorCode, ErrorText);
        }
    }
}

