using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Application.Commands.Seed;
using TypeFileAggregate = Genial.Cms.Domain.Aggregates.TypeFile;
using Genial.Cms.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Genial.Cms.Application.CommandHandlers.Seed;

public class SeedTypeFilesCommandHandler : IRequestHandler<SeedTypeFilesCommand, SeedTypeFilesCommandResult>
{
    private readonly ITypeFileRepository _typeFileRepository;
    private readonly ILogger<SeedTypeFilesCommandHandler> _logger;

    // Categorias de extensões de arquivo - definidas como estáticas para melhor performance
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".png", ".svg", ".gif", ".webp"
    };

    private static readonly HashSet<string> DocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx"
    };

    private static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".avi", ".mov", ".3gp"
    };

    private static readonly HashSet<string> ArchiveExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".zip", ".7z", ".rar"
    };

    public SeedTypeFilesCommandHandler(
        ITypeFileRepository typeFileRepository,
        ILogger<SeedTypeFilesCommandHandler> logger)
    {
        _typeFileRepository = typeFileRepository;
        _logger = logger;
    }

    public async Task<SeedTypeFilesCommandResult> Handle(SeedTypeFilesCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando seed de TypeFiles");

        var typeFilesToSeed = new List<TypeFileAggregate>
        {
            new TypeFileAggregate { Key = "N/A", Value = "application/octet-stream", Order = 1, Category = GetCategory("N/A"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".xpm", Value = "image/x-xpixmap", Order = 2, Category = GetCategory(".xpm"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".7z", Value = "application/x-7z-compressed", Order = 3, Category = GetCategory(".7z"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".zip", Value = "application/zip", Order = 4, Category = GetCategory(".zip"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".xlsx", Value = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", Order = 5, Category = GetCategory(".xlsx"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".docx", Value = "application/vnd.openxmlformats-officedocument.wordprocessingml.document", Order = 6, Category = GetCategory(".docx"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".pptx", Value = "application/vnd.openxmlformats-officedocument.presentationml.presentation", Order = 7, Category = GetCategory(".pptx"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".epub", Value = "application/epub+zip", Order = 8, Category = GetCategory(".epub"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".apk", Value = "application/vnd.android.package-archive", Order = 9, Category = GetCategory(".apk"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".jar", Value = "application/jar", Order = 10, Category = GetCategory(".jar"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".odt", Value = "application/vnd.oasis.opendocument.text", Order = 11, Category = GetCategory(".odt"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".ott", Value = "application/vnd.oasis.opendocument.text-template", Order = 12, Category = GetCategory(".ott"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".ods", Value = "application/vnd.oasis.opendocument.spreadsheet", Order = 13, Category = GetCategory(".ods"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".ots", Value = "application/vnd.oasis.opendocument.spreadsheet-template", Order = 14, Category = GetCategory(".ots"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".odp", Value = "application/vnd.oasis.opendocument.presentation", Order = 15, Category = GetCategory(".odp"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".otp", Value = "application/vnd.oasis.opendocument.presentation-template", Order = 16, Category = GetCategory(".otp"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".odg", Value = "application/vnd.oasis.opendocument.graphics", Order = 17, Category = GetCategory(".odg"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".otg", Value = "application/vnd.oasis.opendocument.graphics-template", Order = 18, Category = GetCategory(".otg"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".odf", Value = "application/vnd.oasis.opendocument.formula", Order = 19, Category = GetCategory(".odf"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".odc", Value = "application/vnd.oasis.opendocument.chart", Order = 20, Category = GetCategory(".odc"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".sxc", Value = "application/vnd.sun.xml.calc", Order = 21, Category = GetCategory(".sxc"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".pdf", Value = "application/pdf", Order = 22, Category = GetCategory(".pdf"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".fdf", Value = "application/vnd.fdf", Order = 23, Category = GetCategory(".fdf"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = "N/A", Value = "application/x-ole-storage", Order = 24, Category = GetCategory("N/A"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".msi", Value = "application/x-ms-installer", Order = 25, Category = GetCategory(".msi"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".aaf", Value = "application/octet-stream", Order = 26, Category = GetCategory(".aaf"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".msg", Value = "application/vnd.ms-outlook", Order = 27, Category = GetCategory(".msg"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".xls", Value = "application/vnd.ms-excel", Order = 28, Category = GetCategory(".xls"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".pub", Value = "application/vnd.ms-publisher", Order = 29, Category = GetCategory(".pub"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".ppt", Value = "application/vnd.ms-powerpoint", Order = 30, Category = GetCategory(".ppt"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".doc", Value = "application/msword", Order = 31, Category = GetCategory(".doc"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".ps", Value = "application/postscript", Order = 32, Category = GetCategory(".ps"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".psd", Value = "image/vnd.adobe.photoshop", Order = 33, Category = GetCategory(".psd"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".p7s", Value = "application/pkcs7-signature", Order = 34, Category = GetCategory(".p7s"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".ogg", Value = "application/ogg", Order = 35, Category = GetCategory(".ogg"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".oga", Value = "audio/ogg", Order = 36, Category = GetCategory(".oga"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".ogv", Value = "video/ogg", Order = 37, Category = GetCategory(".ogv"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".png", Value = "image/png", Order = 38, Category = GetCategory(".png"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".png", Value = "image/vnd.mozilla.apng", Order = 39, Category = GetCategory(".png"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".jpg", Value = "image/jpeg", Order = 40, Category = GetCategory(".jpg"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".jxl", Value = "image/jxl", Order = 41, Category = GetCategory(".jxl"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".jp2", Value = "image/jp2", Order = 42, Category = GetCategory(".jp2"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".jpf", Value = "image/jpx", Order = 43, Category = GetCategory(".jpf"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".jpm", Value = "image/jpm", Order = 44, Category = GetCategory(".jpm"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".jxs", Value = "image/jxs", Order = 45, Category = GetCategory(".jxs"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".gif", Value = "image/gif", Order = 46, Category = GetCategory(".gif"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".webp", Value = "image/webp", Order = 47, Category = GetCategory(".webp"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".exe", Value = "application/vnd.microsoft.portable-executable", Order = 48, Category = GetCategory(".exe"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = "N/A", Value = "application/x-elf", Order = 49, Category = GetCategory("N/A"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = "N/A", Value = "application/x-object", Order = 50, Category = GetCategory("N/A"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = "N/A", Value = "application/x-executable", Order = 51, Category = GetCategory("N/A"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".so", Value = "application/x-sharedlib", Order = 52, Category = GetCategory(".so"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = "N/A", Value = "application/x-coredump", Order = 53, Category = GetCategory("N/A"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".a", Value = "application/x-archive", Order = 54, Category = GetCategory(".a"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".deb", Value = "application/vnd.debian.binary-package", Order = 55, Category = GetCategory(".deb"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".tar", Value = "application/x-tar", Order = 56, Category = GetCategory(".tar"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".xar", Value = "application/x-xar", Order = 57, Category = GetCategory(".xar"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".bz2", Value = "application/x-bzip2", Order = 58, Category = GetCategory(".bz2"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".fits", Value = "application/fits", Order = 59, Category = GetCategory(".fits"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".tiff", Value = "image/tiff", Order = 60, Category = GetCategory(".tiff"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".bmp", Value = "image/bmp", Order = 61, Category = GetCategory(".bmp"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".ico", Value = "image/x-icon", Order = 62, Category = GetCategory(".ico"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".mp3", Value = "audio/mpeg", Order = 63, Category = GetCategory(".mp3"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".flac", Value = "audio/flac", Order = 64, Category = GetCategory(".flac"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".midi", Value = "audio/midi", Order = 65, Category = GetCategory(".midi"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".ape", Value = "audio/ape", Order = 66, Category = GetCategory(".ape"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".mpc", Value = "audio/musepack", Order = 67, Category = GetCategory(".mpc"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".amr", Value = "audio/amr", Order = 68, Category = GetCategory(".amr"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".wav", Value = "audio/wav", Order = 69, Category = GetCategory(".wav"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".aiff", Value = "audio/aiff", Order = 70, Category = GetCategory(".aiff"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".au", Value = "audio/basic", Order = 71, Category = GetCategory(".au"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".mpeg", Value = "video/mpeg", Order = 72, Category = GetCategory(".mpeg"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".mov", Value = "video/quicktime", Order = 73, Category = GetCategory(".mov"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".mp4", Value = "video/mp4", Order = 74, Category = GetCategory(".mp4"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".avif", Value = "image/avif", Order = 75, Category = GetCategory(".avif"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".3gp", Value = "video/3gpp", Order = 76, Category = GetCategory(".3gp"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".3g2", Value = "video/3gpp2", Order = 77, Category = GetCategory(".3g2"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".mp4", Value = "audio/mp4", Order = 78, Category = GetCategory(".mp4"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".mqv", Value = "video/quicktime", Order = 79, Category = GetCategory(".mqv"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".m4a", Value = "audio/x-m4a", Order = 80, Category = GetCategory(".m4a"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".m4v", Value = "video/x-m4v", Order = 81, Category = GetCategory(".m4v"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".heic", Value = "image/heic", Order = 82, Category = GetCategory(".heic"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".heic", Value = "image/heic-sequence", Order = 83, Category = GetCategory(".heic"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".heif", Value = "image/heif", Order = 84, Category = GetCategory(".heif"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".heif", Value = "image/heif-sequence", Order = 85, Category = GetCategory(".heif"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".mj2", Value = "video/mj2", Order = 86, Category = GetCategory(".mj2"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".dvb", Value = "video/vnd.dvb.file", Order = 87, Category = GetCategory(".dvb"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".webm", Value = "video/webm", Order = 88, Category = GetCategory(".webm"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".avi", Value = "video/x-msvideo", Order = 89, Category = GetCategory(".avi"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".flv", Value = "video/x-flv", Order = 90, Category = GetCategory(".flv"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".mkv", Value = "video/x-matroska", Order = 91, Category = GetCategory(".mkv"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".asf", Value = "video/x-ms-asf", Order = 92, Category = GetCategory(".asf"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".aac", Value = "audio/aac", Order = 93, Category = GetCategory(".aac"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".voc", Value = "audio/x-unknown", Order = 94, Category = GetCategory(".voc"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".m3u", Value = "application/vnd.apple.mpegurl", Order = 95, Category = GetCategory(".m3u"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".rmvb", Value = "application/vnd.rn-realmedia-vbr", Order = 96, Category = GetCategory(".rmvb"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".gz", Value = "application/gzip", Order = 97, Category = GetCategory(".gz"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".class", Value = "application/x-java-applet", Order = 98, Category = GetCategory(".class"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".swf", Value = "application/x-shockwave-flash", Order = 99, Category = GetCategory(".swf"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".crx", Value = "application/x-chrome-extension", Order = 100, Category = GetCategory(".crx"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".ttf", Value = "font/ttf", Order = 101, Category = GetCategory(".ttf"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".woff", Value = "font/woff", Order = 102, Category = GetCategory(".woff"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".woff2", Value = "font/woff2", Order = 103, Category = GetCategory(".woff2"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".otf", Value = "font/otf", Order = 104, Category = GetCategory(".otf"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".ttc", Value = "font/collection", Order = 105, Category = GetCategory(".ttc"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".eot", Value = "application/vnd.ms-fontobject", Order = 106, Category = GetCategory(".eot"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".wasm", Value = "application/wasm", Order = 107, Category = GetCategory(".wasm"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".shx", Value = "application/vnd.shx", Order = 108, Category = GetCategory(".shx"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".shp", Value = "application/vnd.shp", Order = 109, Category = GetCategory(".shp"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".dbf", Value = "application/x-dbf", Order = 110, Category = GetCategory(".dbf"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".dcm", Value = "application/dicom", Order = 111, Category = GetCategory(".dcm"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".rar", Value = "application/x-rar-compressed", Order = 112, Category = GetCategory(".rar"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".djvu", Value = "image/vnd.djvu", Order = 113, Category = GetCategory(".djvu"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".mobi", Value = "application/x-mobipocket-ebook", Order = 114, Category = GetCategory(".mobi"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".lit", Value = "application/x-ms-reader", Order = 115, Category = GetCategory(".lit"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".bpg", Value = "image/bpg", Order = 116, Category = GetCategory(".bpg"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".cbor", Value = "application/cbor", Order = 117, Category = GetCategory(".cbor"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".sqlite", Value = "application/vnd.sqlite3", Order = 118, Category = GetCategory(".sqlite"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".dwg", Value = "image/vnd.dwg", Order = 119, Category = GetCategory(".dwg"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".nes", Value = "application/vnd.nintendo.snes.rom", Order = 120, Category = GetCategory(".nes"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".lnk", Value = "application/x-ms-shortcut", Order = 121, Category = GetCategory(".lnk"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".macho", Value = "application/x-mach-binary", Order = 122, Category = GetCategory(".macho"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".qcp", Value = "audio/qcelp", Order = 123, Category = GetCategory(".qcp"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".icns", Value = "image/x-icns", Order = 124, Category = GetCategory(".icns"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".hdr", Value = "image/vnd.radiance", Order = 125, Category = GetCategory(".hdr"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".mrc", Value = "application/marc", Order = 126, Category = GetCategory(".mrc"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".mdb", Value = "application/x-msaccess", Order = 127, Category = GetCategory(".mdb"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".accdb", Value = "application/x-msaccess", Order = 128, Category = GetCategory(".accdb"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".zst", Value = "application/zstd", Order = 129, Category = GetCategory(".zst"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".cab", Value = "application/vnd.ms-cab-compressed", Order = 130, Category = GetCategory(".cab"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".rpm", Value = "application/x-rpm", Order = 131, Category = GetCategory(".rpm"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".xz", Value = "application/x-xz", Order = 132, Category = GetCategory(".xz"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".lz", Value = "application/lzip", Order = 133, Category = GetCategory(".lz"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".torrent", Value = "application/x-bittorrent", Order = 134, Category = GetCategory(".torrent"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".cpio", Value = "application/x-cpio", Order = 135, Category = GetCategory(".cpio"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = "N/A", Value = "application/tzif", Order = 136, Category = GetCategory("N/A"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".xcf", Value = "image/x-xcf", Order = 137, Category = GetCategory(".xcf"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".pat", Value = "image/x-gimp-pat", Order = 138, Category = GetCategory(".pat"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".gbr", Value = "image/x-gimp-gbr", Order = 139, Category = GetCategory(".gbr"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".glb", Value = "model/gltf-binary", Order = 140, Category = GetCategory(".glb"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".cab", Value = "application/x-installshield", Order = 141, Category = GetCategory(".cab"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".jxr", Value = "image/jxr", Order = 142, Category = GetCategory(".jxr"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".parquet", Value = "application/vnd.apache.parquet", Order = 143, Category = GetCategory(".parquet"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".txt", Value = "text/plain", Order = 144, Category = GetCategory(".txt"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".html", Value = "text/html", Order = 145, Category = GetCategory(".html"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".svg", Value = "image/svg+xml", Order = 146, Category = GetCategory(".svg"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".xml", Value = "text/xml", Order = 147, Category = GetCategory(".xml"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".rss", Value = "application/rss+xml", Order = 148, Category = GetCategory(".rss"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".atom", Value = "application/atom+xml", Order = 149, Category = GetCategory(".atom"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".x3d", Value = "model/x3d+xml", Order = 150, Category = GetCategory(".x3d"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".kml", Value = "application/vnd.google-earth.kml+xml", Order = 151, Category = GetCategory(".kml"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".xlf", Value = "application/x-xliff+xml", Order = 152, Category = GetCategory(".xlf"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".dae", Value = "model/vnd.collada+xml", Order = 153, Category = GetCategory(".dae"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".gml", Value = "application/gml+xml", Order = 154, Category = GetCategory(".gml"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".gpx", Value = "application/gpx+xml", Order = 155, Category = GetCategory(".gpx"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".tcx", Value = "application/vnd.garmin.tcx+xml", Order = 156, Category = GetCategory(".tcx"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".amf", Value = "application/x-amf", Order = 157, Category = GetCategory(".amf"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".3mf", Value = "application/vnd.ms-package.3dmanufacturing-3dmodel+xml", Order = 158, Category = GetCategory(".3mf"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".xfdf", Value = "application/vnd.adobe.xfdf", Order = 159, Category = GetCategory(".xfdf"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".owl", Value = "application/owl+xml", Order = 160, Category = GetCategory(".owl"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".php", Value = "text/x-php", Order = 161, Category = GetCategory(".php"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".js", Value = "text/javascript", Order = 162, Category = GetCategory(".js"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".lua", Value = "text/x-lua", Order = 163, Category = GetCategory(".lua"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".pl", Value = "text/x-perl", Order = 164, Category = GetCategory(".pl"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".py", Value = "text/x-python", Order = 165, Category = GetCategory(".py"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".json", Value = "application/json", Order = 166, Category = GetCategory(".json"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".geojson", Value = "application/geo+json", Order = 167, Category = GetCategory(".geojson"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".har", Value = "application/json", Order = 168, Category = GetCategory(".har"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".ndjson", Value = "application/x-ndjson", Order = 169, Category = GetCategory(".ndjson"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".rtf", Value = "text/rtf", Order = 170, Category = GetCategory(".rtf"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".srt", Value = "application/x-subrip", Order = 171, Category = GetCategory(".srt"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".tcl", Value = "text/x-tcl", Order = 172, Category = GetCategory(".tcl"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".csv", Value = "text/csv", Order = 173, Category = GetCategory(".csv"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".tsv", Value = "text/tab-separated-values", Order = 174, Category = GetCategory(".tsv"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".vcf", Value = "text/vcard", Order = 175, Category = GetCategory(".vcf"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".ics", Value = "text/calendar", Order = 176, Category = GetCategory(".ics"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".warc", Value = "application/warc", Order = 177, Category = GetCategory(".warc"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TypeFileAggregate { Key = ".vtt", Value = "text/vtt", Order = 178, Category = GetCategory(".vtt"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        var createdCount = 0;
        var existingCount = 0;

        foreach (var typeFile in typeFilesToSeed)
        {
            // Verificar se o typeFile já existe pela key e value (pois pode haver keys duplicadas como "N/A")
            var existingTypeFile = await _typeFileRepository.GetByKeyAndValueAsync(typeFile.Key, typeFile.Value, cancellationToken);

            if (existingTypeFile != null)
            {
                _logger.LogInformation("TypeFile já existe. Key: {Key}, Value: {Value}, Id: {Id}", typeFile.Key, typeFile.Value, existingTypeFile.Id);
                existingCount++;
                continue;
            }

            // Inserir typeFile no MongoDB
            await _typeFileRepository.InsertAsync(typeFile, cancellationToken);
            _logger.LogInformation("TypeFile criado com sucesso. Key: {Key}, Value: {Value}, Id: {Id}", typeFile.Key, typeFile.Value, typeFile.Id);
            createdCount++;
        }

        var message = $"TypeFiles: {createdCount} criado(s), {existingCount} já existente(s).";

        _logger.LogInformation("Seed de TypeFiles concluído. {Message}", message);

        return new SeedTypeFilesCommandResult
        {
            Success = true,
            Message = message
        };
    }

    private static string GetCategory(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || key == "N/A")
            return null;

        // Usar StringComparer.OrdinalIgnoreCase já definido nos HashSets
        // Não precisa converter para lowercase manualmente
        if (ImageExtensions.Contains(key))
            return "Images";

        if (DocumentExtensions.Contains(key))
            return "Documents";

        if (VideoExtensions.Contains(key))
            return "Videos";

        if (ArchiveExtensions.Contains(key))
            return "Archives";

        return null;
    }
}
