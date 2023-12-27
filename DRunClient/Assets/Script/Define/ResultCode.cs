using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Festa.Client
{
    public static class ResultCode
    {
        public static int ok = 0;
        public static int error = 1;

        public static int error_http_call_error = 10000;

        public static int error_invalid_token = 2;
        public static int error_not_exists = 3;
		public static int error_wrong_password = 5;
		public static int error_username_already_exists = 100;

        public static int error_email_used_by_other = 103;
        public static int error_already = 106;
		public static int error_device_already = 110;
	}

}

