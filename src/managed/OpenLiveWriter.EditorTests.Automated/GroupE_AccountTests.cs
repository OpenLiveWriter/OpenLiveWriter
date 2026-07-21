// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenLiveWriter.EditorTests.Automated.Publish;
using OpenLiveWriter.Publishing;
using OpenLiveWriter.Publishing.Accounts;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group E — blog accounts + account-aware publishing. Exercises the cross-platform
    /// account model/store, the credential seam (via the in-memory fake — never the real
    /// Keychain / <c>security</c> CLI), current-blog selection persistence, and the full
    /// publish-command flow through <see cref="BlogAccountService"/> with a
    /// <see cref="FakeBlogClient"/> standing in for the network transport. All headless.
    /// </summary>
    [TestFixture]
    [Category("GroupE")]
    public class GroupE_AccountTests
    {
        private string _dir;

        [SetUp]
        public void SetUp()
        {
            _dir = Path.Combine(Path.GetTempPath(), "OLWAccountTests", Guid.NewGuid().ToString("N"));
        }

        [TearDown]
        public void TearDown()
        {
            try { if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true); }
            catch { /* best effort */ }
        }

        private FileAccountStore NewStore() => new FileAccountStore(_dir);

        private static BlogAccount NewAccount(string name = "My Blog") => new BlogAccount
        {
            DisplayName = name,
            HomepageUrl = "https://blog.example.com",
            ApiEndpointUrl = "https://blog.example.com/xmlrpc.php",
            BlogId = "blog-1",
            Username = "author"
        };

        // ---- Account store round-trip ----

        [Test]
        public void AccountStore_SaveAssignsId_AndRoundTrips()
        {
            var store = NewStore();
            BlogAccount saved = store.Save(NewAccount());

            Assert.That(saved.Id, Is.Not.Empty, "Save should assign an id to a new account");

            BlogAccount loaded = store.Load(saved.Id);
            Assert.That(loaded, Is.Not.Null);
            Assert.That(loaded.DisplayName, Is.EqualTo("My Blog"));
            Assert.That(loaded.ApiEndpointUrl, Is.EqualTo("https://blog.example.com/xmlrpc.php"));
            Assert.That(loaded.BlogId, Is.EqualTo("blog-1"));
            Assert.That(loaded.Username, Is.EqualTo("author"));
            Assert.That(loaded.ProviderType, Is.EqualTo(BlogAccount.DefaultProviderType));
        }

        [Test]
        public void AccountStore_Save_DoesNotPersistPassword()
        {
            var store = NewStore();
            BlogAccount saved = store.Save(NewAccount());

            // The account JSON must never contain a secret — passwords live in the
            // credential store, not on disk here.
            string json = File.ReadAllText(
                Path.Combine(_dir, saved.Id + ".olaccount.json"));
            Assert.That(json, Does.Not.Contain("password"));
            Assert.That(json, Does.Not.Contain("Password"));
        }

        [Test]
        public void AccountStore_Overwrite_KeepsSingleFile()
        {
            var store = NewStore();
            BlogAccount saved = store.Save(NewAccount());
            saved.DisplayName = "Renamed";
            store.Save(saved);

            var files = Directory.EnumerateFiles(_dir, "*.olaccount.json").ToList();
            Assert.That(files.Count, Is.EqualTo(1));
            Assert.That(store.Load(saved.Id).DisplayName, Is.EqualTo("Renamed"));
        }

        [Test]
        public void AccountStore_List_ReturnsAll_AndDelete_Removes()
        {
            var store = NewStore();
            BlogAccount a = store.Save(NewAccount("Alpha"));
            BlogAccount b = store.Save(NewAccount("Beta"));

            Assert.That(store.List().Select(x => x.Id), Is.EquivalentTo(new[] { a.Id, b.Id }));

            store.Delete(a.Id);
            Assert.That(store.Exists(a.Id), Is.False);
            Assert.That(store.List().Select(x => x.Id), Is.EquivalentTo(new[] { b.Id }));
        }

        [Test]
        public void AccountStore_MissingDirectory_IsEmptyStore()
        {
            var store = NewStore(); // dir does not exist yet
            Assert.That(store.List(), Is.Empty);
            Assert.That(store.Load("nope"), Is.Null);
            Assert.That(store.Exists("nope"), Is.False);
        }

        [Test]
        public void AccountStore_CorruptFile_SkippedByList_ButLoadThrows()
        {
            var store = NewStore();
            BlogAccount good = store.Save(NewAccount("Good"));

            // Write a corrupt account file directly.
            string badId = "corrupt";
            File.WriteAllText(Path.Combine(_dir, badId + ".olaccount.json"), "{ this is not json ");

            // List skips the corrupt entry but keeps the good one.
            Assert.That(store.List().Select(x => x.Id), Is.EquivalentTo(new[] { good.Id }));

            // Load surfaces the corruption explicitly.
            Assert.Throws<AccountStoreException>(() => store.Load(badId));
        }

        // ---- Current-blog selection persistence ----

        [Test]
        public void AccountStore_CurrentSelection_PersistsAcrossInstances()
        {
            var store = NewStore();
            BlogAccount a = store.Save(NewAccount("Alpha"));
            BlogAccount b = store.Save(NewAccount("Beta"));

            store.CurrentAccountId = b.Id;

            // A fresh store reading the same directory sees the persisted selection.
            var reopened = new FileAccountStore(_dir);
            Assert.That(reopened.CurrentAccountId, Is.EqualTo(b.Id));

            // Deleting the current account clears the pointer.
            reopened.Delete(b.Id);
            Assert.That(reopened.CurrentAccountId, Is.Null);
        }

        [Test]
        public void AccountStore_CorruptCurrentPointer_ResolvesToNone()
        {
            var store = NewStore();
            store.Save(NewAccount());
            Directory.CreateDirectory(_dir);
            File.WriteAllText(Path.Combine(_dir, "current.json"), "not json");
            Assert.That(store.CurrentAccountId, Is.Null);
        }

        // ---- Credential seam (fake) ----

        [Test]
        public void CredentialStore_Fake_StoreRetrieveDelete()
        {
            ICredentialStore creds = new InMemoryCredentialStore();
            Assert.That(creds.Exists("k1"), Is.False);
            Assert.That(creds.Retrieve("k1"), Is.Null);

            creds.Store("k1", "author", "s3cret");
            Assert.That(creds.Exists("k1"), Is.True);
            var got = creds.Retrieve("k1");
            Assert.That(got, Is.Not.Null);
            Assert.That(got.Value.Username, Is.EqualTo("author"));
            Assert.That(got.Value.Password, Is.EqualTo("s3cret"));

            creds.Store("k1", "author", "updated"); // overwrite
            Assert.That(creds.Retrieve("k1").Value.Password, Is.EqualTo("updated"));

            creds.Delete("k1");
            Assert.That(creds.Exists("k1"), Is.False);
        }

        // ---- BlogAccountService: metadata + credential together ----

        [Test]
        public void Service_SaveAccount_StoresMetadataAndPassword_AndSetsCurrent()
        {
            var creds = new InMemoryCredentialStore();
            var service = new BlogAccountService(NewStore(), creds);

            BlogAccount saved = service.SaveAccount(NewAccount(), "hunter2");

            Assert.That(saved.Id, Is.Not.Empty);
            Assert.That(service.HasAccounts, Is.True);
            Assert.That(service.GetPassword(saved.Id), Is.EqualTo("hunter2"));
            // First account becomes current automatically.
            Assert.That(service.CurrentAccount?.Id, Is.EqualTo(saved.Id));
        }

        [Test]
        public void Service_MetadataOnlyUpdate_KeepsExistingPassword()
        {
            var creds = new InMemoryCredentialStore();
            var service = new BlogAccountService(NewStore(), creds);
            BlogAccount saved = service.SaveAccount(NewAccount(), "keepme");

            saved.DisplayName = "Renamed";
            service.SaveAccount(saved, password: null); // null = leave secret untouched

            Assert.That(service.GetPassword(saved.Id), Is.EqualTo("keepme"));
            Assert.That(service.GetAccount(saved.Id).DisplayName, Is.EqualTo("Renamed"));
        }

        [Test]
        public void Service_DeleteAccount_RemovesCredential()
        {
            var creds = new InMemoryCredentialStore();
            var service = new BlogAccountService(NewStore(), creds);
            BlogAccount saved = service.SaveAccount(NewAccount(), "pw");

            service.DeleteAccount(saved.Id);
            Assert.That(service.HasAccounts, Is.False);
            Assert.That(creds.Exists(saved.Id), Is.False);
        }

        [Test]
        public void Service_SetCurrentAccount_Persists_AndIgnoresUnknownId()
        {
            var service = new BlogAccountService(NewStore(), new InMemoryCredentialStore());
            BlogAccount a = service.SaveAccount(NewAccount("Alpha"), "pw");
            BlogAccount b = service.SaveAccount(NewAccount("Beta"), "pw");

            service.SetCurrentAccount(b.Id);
            Assert.That(service.CurrentAccount?.Id, Is.EqualTo(b.Id));

            service.SetCurrentAccount("does-not-exist"); // ignored
            Assert.That(service.CurrentAccount?.Id, Is.EqualTo(b.Id));
        }

        // ---- Full publish-command flow (FakeBlogClient) ----

        private static BlogAccountService ServiceWithFake(out FakeBlogClient fake, string dir)
        {
            var captured = new FakeBlogClient();
            fake = captured;
            var store = new FileAccountStore(dir);
            var creds = new InMemoryCredentialStore();
            // Inject a factory that returns our capturing fake instead of a real client.
            return new BlogAccountService(store, creds, (account, password) => captured);
        }

        [Test]
        public async Task Publish_Publish_CallsNewPostWithCorrectPayloadAndPublishTrue()
        {
            BlogAccountService service = ServiceWithFake(out FakeBlogClient fake, _dir);
            BlogAccount account = NewAccount();
            account.BlogId = "blog-42";
            BlogAccount saved = service.SaveAccount(account, "pw");
            service.SetCurrentAccount(saved.Id);

            var doc = new PostDocument { Title = "My Post" };
            doc.Categories.Add("News");
            doc.Categories.Add("Updates");

            string html = "<p>Intro</p>" + ExtendedEntry.BreakMarker + "<p>The rest</p>";
            PublishOutcome outcome = await service.PublishAsync(doc, html, publish: true);

            Assert.That(outcome.Succeeded, Is.True);
            Assert.That(fake.NewPostCount, Is.EqualTo(1));
            Assert.That(fake.LastBlogId, Is.EqualTo("blog-42"));
            Assert.That(fake.LastPublish, Is.True);

            BlogPost post = fake.LastPost;
            Assert.That(post.Title, Is.EqualTo("My Post"));
            Assert.That(post.MainContents, Is.EqualTo("<p>Intro</p>"));
            Assert.That(post.ExtendedContents, Is.EqualTo("<p>The rest</p>")); // mt_text_more
            Assert.That(post.Categories, Is.EquivalentTo(new[] { "News", "Updates" }));
            Assert.That(post.IsPublished, Is.True);

            // The returned server post id is recorded on the document.
            Assert.That(doc.PublishedPostId, Is.EqualTo(outcome.PostId));
            Assert.That(doc.PublishedPostId, Is.EqualTo("fake-post-1"));
            Assert.That(doc.BlogId, Is.EqualTo("blog-42"));
        }

        [Test]
        public async Task Publish_AsDraft_CallsNewPostWithPublishFalse()
        {
            BlogAccountService service = ServiceWithFake(out FakeBlogClient fake, _dir);
            BlogAccount saved = service.SaveAccount(NewAccount(), "pw");
            service.SetCurrentAccount(saved.Id);

            var doc = new PostDocument { Title = "WIP" };
            PublishOutcome outcome = await service.PublishAsync(doc, "<p>Draft body</p>", publish: false);

            Assert.That(outcome.Succeeded, Is.True);
            Assert.That(fake.LastPublish, Is.False);
            Assert.That(fake.LastPost.IsPublished, Is.False);
            Assert.That(fake.LastPost.MainContents, Is.EqualTo("<p>Draft body</p>"));
            Assert.That(doc.IsPublished, Is.False);
        }

        [Test]
        public async Task Publish_NoAccountConfigured_ReturnsGracefully()
        {
            var service = new BlogAccountService(NewStore(), new InMemoryCredentialStore());
            PublishOutcome outcome = await service.PublishAsync(new PostDocument { Title = "x" }, "<p>y</p>", publish: true);

            Assert.That(outcome.Succeeded, Is.False);
            Assert.That(outcome.Status, Is.EqualTo(PublishOutcome.ResultStatus.NoAccountConfigured));
        }

        [Test]
        public async Task Publish_NoStoredCredential_ReturnsNoCredential()
        {
            // Save account metadata but no password (password: null), then publish.
            var store = NewStore();
            var creds = new InMemoryCredentialStore();
            var service = new BlogAccountService(store, creds, (a, p) => new FakeBlogClient());
            BlogAccount saved = service.SaveAccount(NewAccount(), password: null);
            service.SetCurrentAccount(saved.Id);

            PublishOutcome outcome = await service.PublishAsync(new PostDocument { Title = "x" }, "<p>y</p>", publish: true);
            Assert.That(outcome.Status, Is.EqualTo(PublishOutcome.ResultStatus.NoCredential));
        }

        // ---- BlogClientFactory ----

        [Test]
        public void ClientFactory_BuildsMetaWeblogClient_WithAccountOptions()
        {
            var account = NewAccount();
            account.SupportsCategories = false;
            IBlogClient client = BlogClientFactory.CreateClient(account, "pw");

            Assert.That(client, Is.InstanceOf<MetaWeblogXmlRpcClient>());
            Assert.That(client.Options.SupportsCategoriesInline, Is.False);
            Assert.That(client.Options.SupportsExtendedEntries, Is.True);
        }

        [Test]
        public void ClientFactory_UnsupportedProvider_Throws()
        {
            var account = NewAccount();
            account.ProviderType = "AtomPub";
            Assert.Throws<NotSupportedException>(() => BlogClientFactory.CreateClient(account, "pw"));
        }
    }
}
