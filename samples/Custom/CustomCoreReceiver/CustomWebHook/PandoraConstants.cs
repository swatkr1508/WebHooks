using System;
using System.Collections.Generic;
using System.Text;

namespace P112.WebHooks
{
    public static class PandoraConstants
    {
        public static string ReceiverName => "pandora";

        public static string Challenge = "echo";

        public static string SignatureHeaderName => "ms-signature";

        public static int SecretKeyMinLength => 32;
        public static int SecretKeyMaxLength => 64;
    }
}
