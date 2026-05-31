using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechnoDating.Api.Application.Photos.Requests;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Photos;

[ApiController]
[Authorize]
[Route("api/users/me/photos")]
public class PhotosController(IMediator mediator) : ControllerBase
{
    private const long MaxUploadBytes = 10 * 1024 * 1024; // 10 MB
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/webp",
        "image/heic",
        "image/heif",
    };

    [HttpPost]
    [RequestSizeLimit(MaxUploadBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxUploadBytes)]
    public async Task<ActionResult<PhotoDto>> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { error = "file_missing" });
        }
        if (file.Length > MaxUploadBytes)
        {
            return BadRequest(new { error = "file_too_large" });
        }
        if (!AllowedContentTypes.Contains(file.ContentType))
        {
            return BadRequest(new { error = "unsupported_content_type" });
        }

        await using var stream = file.OpenReadStream();
        var result = await mediator.Send(new UploadPhotoRequest(userId, stream, file.ContentType), cancellationToken);
        if (result is null)
        {
            return BadRequest(new { error = "upload_failed" });
        }
        return Ok(result);
    }

    [HttpDelete("{photoId:guid}")]
    public async Task<IActionResult> Delete(Guid photoId, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var removed = await mediator.Send(new DeletePhotoRequest(userId, photoId), cancellationToken);
        return removed ? NoContent() : NotFound();
    }

    [HttpPut("{photoId:guid}/primary")]
    public async Task<ActionResult<PhotoDto>> SetPrimary(Guid photoId, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var result = await mediator.Send(new SetPrimaryPhotoRequest(userId, photoId), cancellationToken);
        if (result is null)
        {
            return NotFound();
        }
        return Ok(result);
    }

    private bool TryGetUserId(out Guid userId)
    {
        var raw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(raw, out userId);
    }
}
