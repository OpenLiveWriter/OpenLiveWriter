// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using OpenLiveWriter.Api;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    public class ImageDecoratorsList : IEnumerable
    {
        private static string APPLIED_DECORATORS = "decoratorsList";
        List<ImageDecorator> _decoratorsList = new List<ImageDecorator>();
        Hashtable _decoratorsTable = new Hashtable();
        BlogPostSettingsBag _decoratorsSettingsBag;
        ImageDecoratorsManager _decoratorsManager;

        internal ImageDecoratorsList(ImageDecoratorsManager decoratorsManager, BlogPostSettingsBag settingsBag)
            : this(decoratorsManager, settingsBag, true)
        {

        }

        internal ImageDecoratorsList(ImageDecoratorsManager decoratorsManager, BlogPostSettingsBag settingsBag, bool addDefaultBorderDecorator)
        {
            _decoratorsManager = decoratorsManager;
            _decoratorsSettingsBag = settingsBag;
            List<ImageDecorator> decorators = new List<ImageDecorator>();

            string appliedDecorators = settingsBag.GetString(APPLIED_DECORATORS, "");
            //Add all of the defined decorators from the settings bag to the decoratorslist.
            //Bug fix note: collect decorators into an arraylist to avoid enumeration modified exception
            //from _decoratorsSettingsBag.SubsettingNames when calling AddDecorator(decorator);
            foreach (string decoratorId in appliedDecorators.Split(','))
            {
                ImageDecorator decorator = decoratorsManager.GetImageDecorator(decoratorId);
                if (decorator != null)
                {
                    decorators.Add(decorator);
                }
            }

            AddDecorator(decorators.ToArray());

            if (addDefaultBorderDecorator && (BorderImageDecorator == null))
                AddDecorator(HtmlBorderDecorator.Id);
        }

        public BlogPostSettingsBag SettingsBag
        {
            get { return _decoratorsSettingsBag; }
        }

        public void MergeDecorators(params string[] decoratorIds)
        {
            List<ImageDecorator> decorators = new List<ImageDecorator>(decoratorIds.Length);
            foreach (string decoratorId in decoratorIds)
            {
                ImageDecorator decorator = _decoratorsManager.GetImageDecorator(decoratorId);

                ImageDecoratorGroup group = _decoratorsManager.GetImageDecoratorsGroup(decorator.GroupName);
                if (group.MutuallyExclusive && _decoratorsList.Exists(d => d.GroupName == decorator.GroupName))
                    continue;

                if (!_decoratorsList.Contains(decorator))
                    decorators.Add(decorator);
            }
            AddDecorator(decorators.ToArray());
        }

        public void AddDecorator(string decoratorId)
        {
            ImageDecorator decorator = _decoratorsManager.GetImageDecorator(decoratorId);
            if (decorator == null)
            {
                Trace.Fail("Attempted to add unregistered decorator " + decoratorId);
                return;
            }
            AddDecorator(decorator);
        }

        public void AddDecorator(params ImageDecorator[] decorators)
        {
            foreach (ImageDecorator decorator in decorators)
            {
                if (!_decoratorsList.Contains(decorator))
                {
                    _decoratorsList.Add(decorator);
                    _decoratorsTable[decorator.Id] = decorator;
                    _decoratorsSettingsBag.CreateSubSettings(decorator.SettingsNamespace);
                }
                else
                    Debug.Fail("Decorator already added: " + decorator.Id);
            }

            EnforceInvariants();
            SaveDecoratorList();
        }

        protected void EnforceInvariants()
        {
            if (_decoratorsList.Count == 0)
                return;

            // Enforce ordering
            ArrayHelper.InsertionSort(_decoratorsList,
                                      delegate (ImageDecorator a, ImageDecorator b) { return Classify(a) - Classify(b); });

            foreach (ImageDecoratorGroup group in _decoratorsManager.ImageDecoratorGroups)
                if (group.MutuallyExclusive)
                    EnforceOneDecoratorForGroup(group);
        }

        protected ImageDecorator EnforceOneDecoratorForGroup(ImageDecoratorGroup group)
        {
            List<ImageDecorator> groupDecorators = new List<ImageDecorator>(group.ImageDecorators);
            List<ImageDecorator> activeDecorators = _decoratorsList.FindAll(d => groupDecorators.Contains(d));
            for (int i = 0; i < activeDecorators.Count - 1; i++)
                _RemoveDecorator(activeDecorators[i]);

            if (activeDecorators.Count == 0)
                return null;
            else
                return activeDecorators[activeDecorators.Count - 1];
        }

        /// <summary>
        /// Specifies the order in which decorators need to appear.
        /// </summary>
        private static int Classify(ImageDecorator x)
        {
            if (x.Id == CropDecorator.Id)
                return -1;
            if (x.IsBorderDecorator)
                return 1;
            if (x.Id == TiltDecorator.Id)
                return 2;

            return 0;
        }

        public Size GetAdjustedOriginalSize(Size originalSize)
        {
            foreach (ImageDecorator decorator in _decoratorsList)
            {
                decorator.AdjustOriginalSize(GetImageDecoratorSettings(decorator), ref originalSize);
            }
            return originalSize;
        }

        private void SaveDecoratorList()
        {
            StringBuilder sb = new StringBuilder();
            foreach (ImageDecorator decorator in _decoratorsList)
            {
                if (sb.Length > 0)
                    sb.Append(",");
                sb.Append(decorator.Id);
            }
            _decoratorsSettingsBag.SetString(APPLIED_DECORATORS, sb.ToString());
        }

        private bool IsSettingsNamespaceInUse(string settingsNamespace)
        {
            foreach (ImageDecorator decorator in _decoratorsList)
            {
                if (decorator.SettingsNamespace == settingsNamespace)
                    return true;
            }
            return false;
        }

        public void RemoveDecorator(ImageDecorator decorator)
        {
            _RemoveDecorator(decorator);

            EnforceInvariants();

            SaveDecoratorList();
        }

        /// <summary>
        /// Removes an decorator without saving the decorators list.
        /// </summary>
        public void _RemoveDecorator(ImageDecorator decorator)
        {
            _decoratorsList.Remove(decorator);
            _decoratorsTable.Remove(decorator.Id);

            //if this was the last decorator assigned to the settings namespace, blow the namespace way
            if (!IsSettingsNamespaceInUse(decorator.SettingsNamespace))
            {
                _decoratorsSettingsBag.RemoveSubSettings(decorator.SettingsNamespace);
            }
        }

        public void RemoveAllDecorators()
        {
            _decoratorsList.Clear();
            _decoratorsTable.Clear();
            _decoratorsSettingsBag.Clear();

            EnforceInvariants();

            SaveDecoratorList();
        }

        public bool ContainsDecorator(string decoratorId)
        {
            return _decoratorsTable.ContainsKey(decoratorId);
        }

        public bool IsDecoratorEditable(string decoratorId)
        {
            ImageDecorator decorator = _decoratorsManager.GetImageDecorator(decoratorId);
            return decorator != null && decorator.IsEditable;
        }

        public ImageDecorator BorderImageDecorator
        {
            get
            {
                return _decoratorsList.Find(d => d.GroupName == ImageDecoratorsManager.BORDER_GROUP);
            }
        }

        public ImageDecorator RecolorImageDecorator
        {
            get
            {
                return _decoratorsList.Find(d => d.GroupName == ImageDecoratorsManager.RECOLOR_GROUP);
            }
        }

        public ImageDecorator SharpenImageDecorator
        {
            get
            {
                return _decoratorsList.Find(d => d.GroupName == ImageDecoratorsManager.SHARPEN_GROUP);
            }
        }

        public ImageDecorator BlurImageDecorator
        {
            get
            {
                return _decoratorsList.Find(d => d.GroupName == ImageDecoratorsManager.BLUR_GROUP);
            }
        }

        public ImageDecorator EmbossImageDecorator
        {
            get
            {
                return _decoratorsList.Find(d => d.GroupName == ImageDecoratorsManager.EMBOSS_GROUP);
            }
        }

        public float? EnforcedAspectRatio
        {
            get
            {
                return BorderImageDecorator != null && BorderImageDecorator.Id == PolaroidBorderDecorator.Id
                        ? PolaroidBorderDecorator.PortalAspectRatio
                        : (float?)null;
            }
        }

        public IProperties GetImageDecoratorSettings(ImageDecorator decorator)
        {
            return new ImageDecoratorSettingsBagAdapter(_decoratorsSettingsBag.CreateSubSettings(decorator.SettingsNamespace));
        }

        public IProperties GetImageDecoratorSettings(string decoratorId)
        {
            ImageDecorator decorator = _decoratorsManager.GetImageDecorator(decoratorId);
            return new ImageDecoratorSettingsBagAdapter(_decoratorsSettingsBag.CreateSubSettings(decorator.SettingsNamespace));
        }

        public ImageDecorator GetImageDecorator(string decoratorId)
        {
            return (ImageDecorator)_decoratorsTable[decoratorId];
        }

        public IEnumerable GetImageDecoratorIds()
        {
            string[] decoratorIds = new string[_decoratorsList.Count];
            for (int i = 0; i < decoratorIds.Length; i++)
            {
                decoratorIds[i] = ((ImageDecorator)_decoratorsList[i]).Id;
            }
            return decoratorIds;
        }

        #region IEnumerable Members
        public IEnumerator GetEnumerator()
        {
            return _decoratorsList.GetEnumerator();
        }

        #endregion
    }
}
