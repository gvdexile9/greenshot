﻿// Greenshot - a free and open source screenshot tool
// Copyright (C) 2007-2018 Thomas Braun, Jens Klingen, Robin Krom
// 
// For more information see: http://getgreenshot.org/
// The Greenshot project is hosted on GitHub https://github.com/greenshot/greenshot
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 1 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autofac.Features.OwnedInstances;
using Dapplo.Addons;
using Dapplo.Log;
using Dapplo.Windows.Clipboard;
using Greenshot.Addon.Tfs.Configuration;
using Greenshot.Addon.Tfs.Entities;
using Greenshot.Addons;
using Greenshot.Addons.Components;
using Greenshot.Addons.Controls;
using Greenshot.Addons.Core;
using Greenshot.Addons.Extensions;
using Greenshot.Addons.Interfaces;
using Greenshot.Gfx;

namespace Greenshot.Addon.Tfs
{
    /// <summary>
    /// This is the TFS destination
    /// </summary>
    [Destination("Tfs")]
    public class TfsDestination : AbstractDestination
    {
        private static readonly LogSource Log = new LogSource();
        private readonly ITfsConfiguration _tfsConfiguration;
        private readonly ITfsLanguage _tfsLanguage;
        private readonly TfsClient _tfsClient;
        private readonly Func<CancellationTokenSource, Owned<PleaseWaitForm>> _pleaseWaitFormFactory;
        private readonly IResourceProvider _resourceProvider;
        private readonly ExportNotification _exportNotification;
        private readonly WorkItem _workItem;

        /// <summary>
        /// DI constructor
        /// </summary>
        /// <param name="coreConfiguration">ICoreConfiguration</param>
        /// <param name="greenshotLanguage">IGreenshotLanguage</param>
        /// <param name="tfsConfiguration">ITfsConfiguration</param>
        /// <param name="tfsLanguage">ITfsLanguage</param>
        /// <param name="tfsClient">TfsClient</param>
        /// <param name="pleaseWaitFormFactory">Func to create please wait forms</param>
        /// <param name="resourceProvider">IResourceProvider</param>
        /// <param name="exportNotification">ExportNotification</param>
        public TfsDestination(
            ICoreConfiguration coreConfiguration,
            IGreenshotLanguage greenshotLanguage,
            ITfsConfiguration tfsConfiguration,
            ITfsLanguage tfsLanguage,
            TfsClient tfsClient,
            Func<CancellationTokenSource, Owned<PleaseWaitForm>> pleaseWaitFormFactory,
            IResourceProvider resourceProvider,
            ExportNotification exportNotification) : base(coreConfiguration, greenshotLanguage)
        {
            _tfsConfiguration = tfsConfiguration;
            _tfsLanguage = tfsLanguage;
            _tfsClient = tfsClient;
            _pleaseWaitFormFactory = pleaseWaitFormFactory;
            _resourceProvider = resourceProvider;
            _exportNotification = exportNotification;
        }

        /// <summary>
        /// DI constructor
        /// </summary>
        /// <param name="coreConfiguration">ICoreConfiguration</param>
        /// <param name="greenshotLanguage">IGreenshotLanguage</param>
        /// <param name="tfsConfiguration">ITfsConfiguration</param>
        /// <param name="tfsLanguage">ITfsLanguage</param>
        /// <param name="tfsClient">TfsClient</param>
        /// <param name="pleaseWaitFormFactory">Func to create please wait forms</param>
        /// <param name="resourceProvider">IResourceProvider</param>
        /// <param name="exportNotification">ExportNotification</param>
        /// <param name="workItem">WorkItem which is exported to</param>
        protected TfsDestination(
            ICoreConfiguration coreConfiguration,
            IGreenshotLanguage greenshotLanguage,
            ITfsConfiguration tfsConfiguration,
            ITfsLanguage tfsLanguage,
            TfsClient tfsClient,
            Func<CancellationTokenSource, Owned<PleaseWaitForm>> pleaseWaitFormFactory,
            IResourceProvider resourceProvider,
            ExportNotification exportNotification,
            WorkItem workItem) :this(coreConfiguration, greenshotLanguage, tfsConfiguration, tfsLanguage, tfsClient, pleaseWaitFormFactory, resourceProvider, exportNotification)
        {
            _workItem = workItem;
        }

        /// <inheritdoc />
        public override bool IsActive => base.IsActive && _tfsClient.CanUpdate;

        /// <inheritdoc />
        public override bool UseDynamicsOnly => true;

        /// <inheritdoc />
        public override bool IsDynamic => true;

        /// <inheritdoc />
        protected override async Task PrepareDynamicDestinations(ToolStripMenuItem destinationToolStripMenuItem)
        {
            if (!destinationToolStripMenuItem.HasDropDownItems)
            {
                var oldColor = destinationToolStripMenuItem.BackColor;
                destinationToolStripMenuItem.BackColor = Color.DarkGray;
                await _tfsClient.UpdateWorkItems();
                destinationToolStripMenuItem.BackColor = oldColor;
            }
        }

        /// <inheritdoc />
        public override IEnumerable<IDestination> DynamicDestinations()
        {
            var workItems = _tfsClient.WorkItems.Values;
            if (workItems.Count == 0)
            {
                yield break;
            }
            foreach (var workItem in workItems)
            {
                yield return new TfsDestination(CoreConfiguration, GreenshotLanguage, _tfsConfiguration, _tfsLanguage, _tfsClient, _pleaseWaitFormFactory, _resourceProvider, _exportNotification, workItem);
            }
        }

        /// <inheritdoc />
        public override string Description
        {
            get
            {
                if (_workItem?.Fields?.Title == null)
                {
                    return _tfsLanguage.UploadMenuItem;
                }
                // Format the title of this destination
                // TODO: substring?
                return $"{_workItem.Fields.WorkItemType} {_workItem.Id}: {_workItem.Fields.Title}";
            }
        }

        /// <inheritdoc />
        public override IBitmapWithNativeSupport DisplayIcon
        {
            get
            {
                // TODO: Optimize this by using a cache
                using (var bitmapStream = _resourceProvider.ResourceAsStream(GetType().Assembly, "vsts.png"))
                {
                    return BitmapHelper.FromStream(bitmapStream);
                }
            }
        }

        /// <inheritdoc />
        public override async Task<ExportInformation> ExportCaptureAsync(bool manuallyInitiated, ISurface surface, ICaptureDetails captureDetails)
        {
            if (_workItem == null)
            {
                return new ExportInformation(Designation, Description)
                {
                    ExportMade = false
                };
            }
            var uploadUrl = await Upload(_workItem, surface);

            var exportInformation = new ExportInformation(Designation, Description)
            {
                ExportMade = uploadUrl != null,
                Uri = uploadUrl?.AbsoluteUri
            };
            _exportNotification.NotifyOfExport(this, exportInformation, surface);
            return exportInformation;
        }

        /// <summary>
        /// Upload the capture to Tfs
        /// </summary>
        /// <param name="workItem">WorkItem</param>
        /// <param name="surfaceToUpload">ISurface</param>
        /// <returns>Uri</returns>
        private async Task<Uri> Upload(WorkItem workItem, ISurface surfaceToUpload)
        {
            try
            {
                Uri response;

                var cancellationTokenSource = new CancellationTokenSource();
                using (var ownedPleaseWaitForm = _pleaseWaitFormFactory(cancellationTokenSource))
                {
                    ownedPleaseWaitForm.Value.SetDetails("TFS plug-in", _tfsLanguage.CommunicationWait);
                    ownedPleaseWaitForm.Value.Show();
                    try
                    {
                        var result = await _tfsClient.CreateAttachment(surfaceToUpload);
                        await _tfsClient.LinkAttachment(workItem, result);
                        response = result.Url;
                    }
                    finally
                    {
                        ownedPleaseWaitForm.Value.Close();
                    }
                }

                if (_tfsConfiguration.AfterUploadLinkToClipBoard)
                {
                    using (var clipboardAccessToken = ClipboardNative.Access())
                    {
                        clipboardAccessToken.ClearContents();
                        clipboardAccessToken.SetAsUrl(response.AbsoluteUri);
                    }
                }

                return response;
            }
            catch (Exception e)
            {
                Log.Error().WriteLine(e, "Error uploading.");
                MessageBox.Show(_tfsLanguage.UploadFailure + " " + e.Message);
            }

            return null;
        }

    }
}