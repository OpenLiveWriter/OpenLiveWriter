// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace OpenLiveWriter.BlogClient
{
    public interface ICredentialsDomain
    {
        string Name { get; }
        string Description { get; }
        byte[] Icon { get; }
        byte[] Image { get; }
        bool AllowsSavePassword { get; }
    }

    public class CredentialsDomain : ICredentialsDomain
    {
        public CredentialsDomain(string name, string description, byte[] icon, byte[] image)
        {
            CredentialsDomainInternal(name, description, icon, image, true);
        }

        public CredentialsDomain(string name, string description, byte[] icon, byte[] image, bool allowSavePassword)
        {
            CredentialsDomainInternal(name, description, icon, image, allowSavePassword);
        }

        private void CredentialsDomainInternal(string name, string description, byte[] icon, byte[] image, bool allowSavePassword)
        {
            _name = name;
            _description = description;
            _icon = icon;
            _image = image;
            _allowsSavePassword = allowSavePassword;
        }

        #region ICredentialsDomain Members

        public string Name
        {
            get { return _name; }
        }
        private string _name;

        public string Description
        {
            get { return _description; }
        }
        private string _description;

        public byte[] Icon
        {
            get { return _icon; }
        }
        private byte[] _icon;

        public byte[] Image
        {
            get { return _image; }
        }
        private byte[] _image;

        public bool AllowsSavePassword
        {
            get { return _allowsSavePassword; }
        }
        private bool _allowsSavePassword;

        #endregion
    }
}
