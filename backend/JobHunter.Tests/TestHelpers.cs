using JobHunter.API.Data;
using JobHunter.API.Models;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.Tests;

public static class TestHelpers
{
    public static JobHunterDbContext NewInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<JobHunterDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new JobHunterDbContext(options);

        db.ThamSos.AddRange(
            new ThamSo { MaThamSo = "TS1", GiaTri = "8" },
            new ThamSo { MaThamSo = "TS2", GiaTri = "5" },
            new ThamSo { MaThamSo = "TS3", GiaTri = "15" },
            new ThamSo { MaThamSo = "TS7", GiaTri = "1" }
        );
        db.SaveChanges();
        return db;
    }
}
