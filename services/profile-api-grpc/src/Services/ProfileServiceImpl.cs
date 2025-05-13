using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ProfileApi.Data;
using ProfileApi.Protos;
using ProfileModel = ProfileApi.Models.Profile;
using ProfileProto = ProfileApi.Protos.Profile;

namespace ProfileApi.Services;

public class ProfileServiceImpl : ProfileManager.ProfileManagerBase
{
    private readonly ProfileDbContext _context;

    public ProfileServiceImpl(ProfileDbContext context)
    {
        _context = context;
    }

    public override async Task<ProfileList> GetAllProfiles(Empty request, ServerCallContext context)
    {
        var profiles = await _context.Profiles.ToListAsync();
        var response = new ProfileList();
        response.Profiles.AddRange(profiles.Select(p => ToProto(p)));
        return response;
    }

    public override async Task<ProfileProto> GetProfile(ProfileId request, ServerCallContext context)
    {
        var profile = await _context.Profiles.FindAsync(request.Id);
        if (profile == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Perfil no encontrado"));
        return ToProto(profile);
    }

    public override async Task<ProfileProto> CreateProfile(ProfileProto request, ServerCallContext context)
    {
        var entity = FromProto(request);
        _context.Profiles.Add(entity);
        await _context.SaveChangesAsync();
        request.Id = entity.Id;
        return request;
    }

    public override async Task<Empty> UpdateProfile(ProfileProto request, ServerCallContext context)
    {
        var profile = await _context.Profiles.FindAsync(request.Id);
        if (profile == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Perfil no encontrado"));

        profile.Name = request.Name;
        profile.Photo = request.Photo;
        profile.Email = request.Email;
        profile.PasswordHash = request.PasswordHash;
        profile.Sex = request.Sex;
        profile.Nickname = request.Nickname;

        await _context.SaveChangesAsync();
        return new Empty();
    }

    public override async Task<Empty> DeleteProfile(ProfileId request, ServerCallContext context)
    {
        var profile = await _context.Profiles.FindAsync(request.Id);
        if (profile == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Perfil no encontrado"));

        _context.Profiles.Remove(profile);
        await _context.SaveChangesAsync();
        return new Empty();
    }

    private ProfileProto ToProto(ProfileModel entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Photo = entity.Photo,
        Email = entity.Email,
        PasswordHash = entity.PasswordHash,
        Sex = entity.Sex,
        Nickname = entity.Nickname
    };

    private ProfileModel FromProto(ProfileProto proto) => new()
    {
        Id = proto.Id,
        Name = proto.Name,
        Photo = proto.Photo,
        Email = proto.Email,
        PasswordHash = proto.PasswordHash,
        Sex = proto.Sex,
        Nickname = proto.Nickname
    };
}
