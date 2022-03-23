using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace APIReciever
{
    public class HashUtility
    {
        public virtual async Task<byte[]> ComputeRequestBodySha256HashAsync(
            HttpRequest request,
            byte[] secret,
            byte[] prefix,
            byte[] suffix)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            if (secret == null)
            {
                throw new ArgumentNullException(nameof(secret));
            }


            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!request.Body.CanSeek)
            {
                request.EnableBuffering();
                Debug.Assert(request.Body.CanSeek);

                await request.Body.DrainAsync(CancellationToken.None);
            }

            // Always start at the beginning.
            request.Body.Seek(0L, SeekOrigin.Begin);

            using (var hasher = new HMACSHA256(secret))
            {
                try
                {
                    if (prefix != null && prefix.Length > 0)
                    {
                        hasher.TransformBlock(
                            inputBuffer: prefix,
                            inputOffset: 0,
                            inputCount: prefix.Length,
                            outputBuffer: null,
                            outputOffset: 0);
                    }

                    // Split body into 4K chunks.
                    var buffer = new byte[4096];
                    var inputStream = request.Body;
                    int bytesRead;
                    while ((bytesRead = await inputStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        hasher.TransformBlock(
                            buffer,
                            inputOffset: 0,
                            inputCount: bytesRead,
                            outputBuffer: null,
                            outputOffset: 0);
                    }

                    if (suffix != null && suffix.Length > 0)
                    {
                        hasher.TransformBlock(
                            suffix,
                            inputOffset: 0,
                            inputCount: suffix.Length,
                            outputBuffer: null,
                            outputOffset: 0);
                    }

                    hasher.TransformFinalBlock(Array.Empty<byte>(), inputOffset: 0, inputCount: 0);

                    return hasher.Hash;
                }
                finally
                {
                    // Reset Position because JsonInputFormatter et cetera always start from current position.
                    request.Body.Seek(0L, SeekOrigin.Begin);
                }
            }
        }
    }
}
