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
            _blogSettings = db.GetCollection<BlogSettings>(nameof(BlogSettings));
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
            var delResult = await _blogSettings.DeleteManyAsync(bs => true);//TODO: this works for now, but would like to delete by Id in the future
            return Result<int>.Success(1);
        }

        public async Task<Result<int>> DeletePostAsync(Post post, CancellationToken cancellationToken)
        {
            FilterDefinition<Post> filterIdDef = Builders<Post>.Filter.Eq(p => p.Id, post.Id);
            var delResult = await _posts.DeleteOneAsync(filterIdDef);
            return Result<int>.Success(1);
        }

        public async Task<Author> GetAuthorByUsernameAsync(string username)
        {
            FilterDefinition<Author> filterUsernameDef = Builders<Author>.Filter.Eq(p => p.UserName, username);
            IAsyncCursor<Author> cursor = await _authors.FindAsync(filterUsernameDef);
            return await cursor.FirstOrDefaultAsync();
        }

        public Task<BlogSettings> GetBlogSettingsAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Post> GetNextPostAsync(Post currentPost, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Post> GetPostByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Post> GetPostBySlugAsync(string slug, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IList<Post>> GetPostListAsync(bool includeUnpublished, Pager pager, string category, PineBlogOptions options, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Post> GetPreviousPostAsync(Post currentPost, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<Post>> GetSyndicationPostsAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Result<Post>> PublishPostAsync(Guid id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Result<Post>> UnpublishPostAsync(Guid id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Result<int>> UpdateBlogSettingsAsync(BlogSettings settings, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Result<int>> UpdatePostAsync(Post post, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
