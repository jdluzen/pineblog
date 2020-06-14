using MongoDB.Driver;
using Opw.PineBlog.Entities;
using Opw.PineBlog.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Opw.PineBlog.Mongo
{
    public class MongoRepository : IRepository
    {
        private IMongoCollection<BlogSettings> _blogSettings;
        private IMongoCollection<Post> _posts;
        private IMongoCollection<Author> _authors;

        public MongoRepository(IMongoClient client, string mongoDbName)
        {
            var db = client.GetDatabase(mongoDbName);
            _blogSettings = db.GetCollection<BlogSettings>(nameof(BlogSettings));//TODO: would like to add a prefix for multiple instances in the same db
            _posts = db.GetCollection<Post>(nameof(Post));
            _authors = db.GetCollection<Author>(nameof(Author));
        }

        public async Task<Result<int>> AddAuthorAsync(Author author, CancellationToken cancellationToken)
        {
            await _authors.InsertOneAsync(author, null, cancellationToken);
            return Result<int>.Success(1);
        }

        public async Task<Result<int>> AddPostAsync(Post post, CancellationToken cancellationToken)
        {
            await _posts.InsertOneAsync(post, null, cancellationToken);
            return Result<int>.Success(1);
        }

        public async Task<Result<int>> DeleteBlogSettingsAsync(BlogSettings settings, CancellationToken cancellationToken)
        {
            //FilterDefinition<BlogSettings> idFilter = Builders<BlogSettings>.Filter.Eq(bs => bs.Id);
            var delResult = await _blogSettings.DeleteOneAsync(bs => true, cancellationToken);//TODO: this works for now, but would like to delete by Id in the future
            return Result<int>.Success(1);
        }

        public async Task<Result<int>> DeletePostAsync(Post post, CancellationToken cancellationToken)
        {
            FilterDefinition<Post> filterIdDef = Builders<Post>.Filter.Eq(p => p.Id, post.Id);
            var delResult = await _posts.DeleteOneAsync(filterIdDef, cancellationToken);
            return Result<int>.Success(1);
        }

        public async Task<Author> GetAuthorByUsernameAsync(string username)
        {
            FilterDefinition<Author> filterUsernameDef = Builders<Author>.Filter.Eq(p => p.UserName, username);
            IAsyncCursor<Author> cursor = await _authors.FindAsync(filterUsernameDef, new FindOptions<Author, Author>
            {
                Limit = 1
            });
            return await cursor.FirstOrDefaultAsync();
        }

        public async Task<BlogSettings> GetBlogSettingsAsync(CancellationToken cancellationToken)
        {
            IAsyncCursor<BlogSettings> cursor = await _blogSettings.FindAsync(bs => true, new FindOptions<BlogSettings, BlogSettings>
            {
                Limit = 1
            }, cancellationToken);
            return await cursor.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Post> GetNextPostAsync(Post currentPost, CancellationToken cancellationToken)
        {
            FilterDefinition<Post> filterPublished = Builders<Post>.Filter.Gt(p => p.Published, currentPost.Published);
            SortDefinition<Post> sortPublished = Builders<Post>.Sort.Ascending(p => p.Published);
            IAsyncCursor<Post> cursor = await _posts.FindAsync(filterPublished, new FindOptions<Post, Post>
            {
                Limit = 1,
                Sort = sortPublished
            }, cancellationToken);

            return await cursor.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Post> GetPostByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            FilterDefinition<Post> filterIdDef = Builders<Post>.Filter.Eq(p => p.Id, id);
            IAsyncCursor<Post> cursor = await _posts.FindAsync(filterIdDef, new FindOptions<Post, Post>
            {
                Limit = 1
            }, cancellationToken);
            return await cursor.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Post> GetPostBySlugAsync(string slug, CancellationToken cancellationToken)
        {
            FilterDefinition<Post> filterSlugDef = Builders<Post>.Filter.Eq(p => p.Slug, slug);
            IAsyncCursor<Post> cursor = await _posts.FindAsync(filterSlugDef, new FindOptions<Post, Post>
            {
                Limit = 1
            }, cancellationToken);
            return await cursor.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IList<Post>> GetPostListAsync(bool includeUnpublished, Pager pager, string category, PineBlogOptions options, CancellationToken cancellationToken)
        {
            if (includeUnpublished)
            {
                FilterDefinition<Post> filterIncludUnpiblished = Builders<Post>.Filter.Ne(p => p.Published, null);

            }
            IAsyncCursor<Post> cursor = await _posts.FindAsync(p => true, new FindOptions<Post, Post>
            {
                Limit = pager.ItemsPerPage,
                Skip = (pager.CurrentPage - 1) * pager.ItemsPerPage
            }, cancellationToken);

            return await cursor.ToListAsync(cancellationToken);
        }

        public async Task<Post> GetPreviousPostAsync(Post currentPost, CancellationToken cancellationToken)
        {
            FilterDefinition<Post> filterPublished = Builders<Post>.Filter.Lt(p => p.Published, currentPost.Published);
            SortDefinition<Post> sortPublished = Builders<Post>.Sort.Descending(p => p.Published);
            IAsyncCursor<Post> cursor = await _posts.FindAsync(filterPublished, new FindOptions<Post, Post>
            {
                Limit = 1,
                Sort = sortPublished
            }, cancellationToken);

            return await cursor.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<Post>> GetSyndicationPostsAsync(CancellationToken cancellationToken)
        {
            FilterDefinition<Post> filterPublished = Builders<Post>.Filter.Ne(p => p.Published, null);
            SortDefinition<Post> sortPublished = Builders<Post>.Sort.Descending(p => p.Published);
            IAsyncCursor<Post> cursor = await _posts.FindAsync(filterPublished, new FindOptions<Post, Post>
            {
                Limit = 25,
                Sort = sortPublished
            }, cancellationToken);

            return await cursor.ToListAsync(cancellationToken);
        }

        public Task<Result<Post>> PublishPostAsync(Guid id, CancellationToken cancellationToken)
        {
            return SetPublishAsync(id, DateTime.UtcNow, cancellationToken);
        }

        public Task<Result<Post>> UnpublishPostAsync(Guid id, CancellationToken cancellationToken)
        {
            return SetPublishAsync(id, null, cancellationToken);
        }
        private async Task<Result<Post>> SetPublishAsync(Guid id, DateTime? published, CancellationToken cancellationToken)
        {
            FilterDefinition<Post> filterIdDef = Builders<Post>.Filter.Eq(p => p.Id, id);
            UpdateDefinition<Post> updatePublishDef = Builders<Post>.Update.Set(p => p.Published, published);
            return await _posts.FindOneAndUpdateAsync(filterIdDef, updatePublishDef, null, cancellationToken) is { } post ? Result<Post>.Success(post) : Result<Post>.Fail();
        }

        public async Task<Result<int>> UpdateBlogSettingsAsync(BlogSettings settings, CancellationToken cancellationToken)
        {
            await _blogSettings.ReplaceOneAsync(bs => true, settings, new ReplaceOptions
            {
                IsUpsert = true
            }, cancellationToken);
            return Result<int>.Success(1);
        }

        public async Task<Result<int>> UpdatePostAsync(Post post, CancellationToken cancellationToken)
        {
            await _posts.ReplaceOneAsync(p => post.Id == post.Id, post, (ReplaceOptions)null, cancellationToken);
            return Result<int>.Success(1);
        }
    }
}
