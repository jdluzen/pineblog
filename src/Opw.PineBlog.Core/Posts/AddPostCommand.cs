using MediatR;
using Opw.HttpExceptions;
using Opw.PineBlog.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Opw.PineBlog.Posts
{
    /// <summary>
    /// Command that adds a post.
    /// </summary>
    public class AddPostCommand : IRequest<Result<Post>>, IEditPostCommand
    {
        /// <summary>
        /// The name of the user adding the post.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The post title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// A short description for the post.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The post content in markdown format.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// A comma separated list of categories.
        /// </summary>
        public string Categories { get; set; }

        /// <summary>
        /// The date the post was published or NULL for unpublished posts.
        /// </summary>
        public DateTime? Published { get; set; }

        /// <summary>
        /// Cover UURLrl.
        /// </summary>
        public string CoverUrl { get; set; }

        /// <summary>
        /// Cover caption.
        /// </summary>
        public string CoverCaption { get; set; }

        /// <summary>
        /// Cover link.
        /// </summary>
        public string CoverLink { get; set; }

        /// <summary>
        /// Handler for the AddPostCommand.
        /// </summary>
        public class Handler : IRequestHandler<AddPostCommand, Result<Post>>
        {
            private readonly IRepository _repo;
            private readonly PostUrlHelper _postUrlHelper;

            /// <summary>
            /// Implementation of AddPostCommand.Handler.
            /// </summary>
            /// <param name="repo">The blog entity repository.</param>
            /// <param name="postUrlHelper">Post URL helper.</param>
            public Handler(IRepository repo, PostUrlHelper postUrlHelper)
            {
                _repo = repo;
                _postUrlHelper = postUrlHelper;
            }

            /// <summary>
            /// Handle the AddPostCommand request.
            /// </summary>
            /// <param name="request">The AddPostCommand request.</param>
            /// <param name="cancellationToken">A cancellation token.</param>
            public async Task<Result<Post>> Handle(AddPostCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var author = await _repo.GetAuthorByUsernameAsync(request.UserName);
                    if (author == null)
                        return Result<Post>.Fail(new NotFoundException($"Could not find author for \"{request.UserName}\"."));

                    var entity = _postUrlHelper.ReplaceBaseUrlWithUrlFormat(new Post
                    {
                        AuthorId = author.Id,
                        Title = request.Title,
                        Slug = request.Title.ToPostSlug(),
                        Description = request.Description,
                        Content = request.Content,
                        Categories = request.Categories,
                        Published = request.Published,
                        CoverUrl = request.CoverUrl,
                        CoverCaption = request.CoverCaption,
                        CoverLink = request.CoverLink
                    });

                    return await _repo.AddPostAsync(entity, cancellationToken) is { } result
                        ? Result<Post>.Success(entity)
                        : Result<Post>.Fail(result.Exception);
                }
                catch (Exception ex)
                {
                    return Result<Post>.Fail(ex);
                }
            }
        }
    }
}
