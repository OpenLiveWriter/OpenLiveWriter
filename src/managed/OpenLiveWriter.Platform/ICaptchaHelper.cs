// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.Platform
{
    /// <summary>
    /// Result of a captcha challenge.
    /// </summary>
    public class CaptchaResult
    {
        public bool Accepted { get; set; }
        public string Reply { get; set; }
    }

    /// <summary>
    /// Platform-agnostic interface for showing a captcha challenge UI.
    /// </summary>
    public interface ICaptchaHelper
    {
        CaptchaResult ShowCaptcha(IBlogClientUIContext uiContext, string captchaImageUrl);
    }
}
