﻿using System.ComponentModel.Composition;
using GitHub.Models;
using GitHub.VisualStudio.Helpers;
using NullGuard;
using Octokit;
using System;
using GitHub.Services;
using GitHub.Api;
using System.Threading.Tasks;
using GitHub.Primitives;
using GitHub.Extensions;

namespace GitHub.VisualStudio.Base
{
    public class TeamExplorerItemBase : TeamExplorerGitRepoInfo, INotifyPropertySource
    {
        readonly ISimpleApiClientFactory apiFactory;
        protected ITeamExplorerServiceHolder holder;

        ISimpleApiClient simpleApiClient;
        [AllowNull]
        public ISimpleApiClient SimpleApiClient
        {
            [return: AllowNull] get { return simpleApiClient; }
            set
            {
                if (simpleApiClient != value && value == null)
                    apiFactory.ClearFromCache(simpleApiClient);
                simpleApiClient = value;
            }
        }

        public TeamExplorerItemBase(ISimpleApiClientFactory apiFactory, ITeamExplorerServiceHolder holder)
        {
            this.apiFactory = apiFactory;
            this.holder = holder;
        }

        public virtual void Execute()
        {
        }

        public virtual void Invalidate()
        {
        }

        protected virtual void RepoChanged()
        {
            var repo = ActiveRepo;
            if (repo != null)
            {
                var gitRepo = Services.GetRepoFromIGit(repo);
                var uri = Services.GetUriFromRepository(gitRepo);
                if (uri != null)
                {
                    var name = uri.GetRepo();
                    if (name != null)
                    {
                        ActiveRepoUri = uri;
                        ActiveRepoName = ActiveRepoUri.GetUser() + "/" + ActiveRepoUri.GetRepo();
                    }
                }
            }
        }

        protected async Task<bool> IsAGitHubRepo()
        {
            var uri = ActiveRepoUri;
            if (uri == null)
                return false;

            SimpleApiClient = apiFactory.Create(uri);

            if (!HostAddress.IsGitHubDotComUri(uri))
            {
                var repo = await SimpleApiClient.GetRepository();
                return repo.FullName == ActiveRepoName && SimpleApiClient.IsEnterprise();
            }
            return true;
        }

        bool isEnabled;
        public bool IsEnabled
        {
            get { return isEnabled; }
            set { isEnabled = value; this.RaisePropertyChange(); }
        }

        bool isVisible;
        public bool IsVisible
        {
            get { return isVisible; }
            set { isVisible = value; this.RaisePropertyChange(); }
        }

        string text;
        [AllowNull]
        public string Text
        {
            get { return text; }
            set { text = value; this.RaisePropertyChange(); }
        }

    }

    [Export(typeof(IGitHubClient))]
    public class GHClient : GitHubClient
    {
        [ImportingConstructor]
        public GHClient(IProgram program)
            : base(program.ProductHeader)
        {

        }
    }
}