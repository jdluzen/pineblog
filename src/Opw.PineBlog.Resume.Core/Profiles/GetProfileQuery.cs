using MediatR;
using Microsoft.EntityFrameworkCore;
using Opw.HttpExceptions;
using Opw.PineBlog.Resume.Entities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Opw.PineBlog.Resume.Profiles
{
    public class GetProfileQuery : IRequest<Result<Profile>>
    {
        public string Slug { get; set; }

        public class Handler : IRequestHandler<GetProfileQuery, Result<Profile>>
        {
            private readonly IResumeEntityDbContext _context;

            public Handler(IResumeEntityDbContext context)
            {
                _context = context;
            }

            public async Task<Result<Profile>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
            {
                var profile = await _context.Profiles
                    .Include(p => p.Education)
                    .Include(p => p.Experiences)
                    .Include(p => p.Languages)
                    .Include(p => p.Links)
                    .Include(p => p.Skills)
                    .Where(p => p.Slug.Equals(request.Slug))
                    .SingleOrDefaultAsync(cancellationToken);

                if (profile == null)
                    return Result<Profile>.Fail(new NotFoundException<Profile>($"Could not find profile for slug: \"{request.Slug}\""));

                return Result<Profile>.Success(profile);
            }
        }
    }
}
