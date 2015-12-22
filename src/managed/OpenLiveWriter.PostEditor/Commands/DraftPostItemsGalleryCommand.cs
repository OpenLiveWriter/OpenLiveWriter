// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.IO;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Commands
{
    public class DraftPostItemsGalleryCommand : DynamicCommandGallery
    {
        private IBlogPostEditingSite postEditingSite;
        private PostInfo[] postInfo;
        private bool _isPost;
        // Note: The maximum number of recent items specified here should match the RecentItem's MaxCount attribute in ribbon.xml.
        private const int MaxItems = PostListCache.MaxItems;
        private Command[] _commands = new Command[MaxItems];
        private object _commandsLock = new object();
        private int draftCmdStart = (int)CommandId.OpenDraftMRU0;

        public DraftPostItemsGalleryCommand(IBlogPostEditingSite postEditingSite, CommandManager commandManager, bool isPost)
            : base((isPost ? CommandId.OpenPostSplit : CommandId.OpenDraftSplit))
        {
            this.postEditingSite = postEditingSite;
            _isPost = isPost;
            if (isPost)
            {
                draftCmdStart = (int)CommandId.OpenPostMRU0;
            }
            lock (_commandsLock)
            {
                // initialize commands
                for (int i = 0; i < _commands.Length; i++)
                {
                    _commands[i] = new Command((CommandId)(i + draftCmdStart));
                    _commands[i].Execute += new EventHandler(DraftPostItemsGalleryCommand_Execute);
                    _commands[i].CommandBarButtonStyle = CommandBarButtonStyle.Provider;
                    _commands[i].On = false;
                }

                // add them to the command manager
                commandManager.Add(new CommandCollection(_commands));
                commandManager.Add(this);
            }
        }

        private void DraftPostItemsGalleryCommand_Execute(object sender, EventArgs e)
        {
            // This is for the gallery commands
            Command command = (Command)sender;
            int commandId = (int)command.CommandId;
            if (commandId >= draftCmdStart && commandId < (draftCmdStart + MaxItems))
            {
                int postIndex = commandId - draftCmdStart;
                WindowCascadeHelper.SetNextOpenedLocation(postEditingSite.FrameWindow.Location);
                postEditingSite.OpenLocalPost(postInfo[postIndex]);
            }
        }

        public override void LoadItems()
        {
            items.Clear();
            selectedIndex = INVALID_INDEX;

            postInfo = (_isPost ? PostListCache.RecentPosts : PostListCache.Drafts);

            lock (_commandsLock)
            {
                for (int i = 0; i < _commands.Length && i < postInfo.Length; i++)
                {
                    PostInfo v = postInfo[i];
                    _commands[i].On = true;
                    _commands[i].Enabled = true;
                    _commands[i].LabelTitle = v.Title;
                    _commands[i].LabelDescription = v.BlogName;
                    _commands[i].TooltipTitle = v.Title;
                    _commands[i].TooltipDescription = v.BlogName;
                    using (BlogSettings bs = BlogSettings.ForBlogId(v.BlogId))
                    {
                        _commands[i].LargeImage = (bs.ClientType.Contains("WordPress") ? Images.WordPressPost_LargeImage
                                                       : Images.OtherBlogPost_LargeImage);
                    }
                    items.Add(new GalleryItem(v.Title, null, _commands[i]));
                }
            }

            base.LoadItems();
        }
    }
}
