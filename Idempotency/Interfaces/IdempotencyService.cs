
using Microsoft.Data.SqlClient;

namespace Idempotency.Interfaces;

public class IdempotencyService(AppDbContext context) : IIdempotencyService
{

    private readonly AppDbContext _context = context;

    

    public async Task<IdempotencyRecord?> GetAsync(string key)
    {
        return await _context
                         .IdempotencyRecords
                         .FirstOrDefaultAsync(r => r.IdempotencyKey == key);
    }

    public async Task SaveResponseAsync(string key, string response, int statusCode)
    {
        var record = await GetAsync(key);

        if (record is null)
        {
            // If the record does not exist (race condition or missing), create it
            record = new IdempotencyRecord
            {
                Id = Guid.NewGuid(),
                IdempotencyKey = key,
                RequestHash = string.Empty,
                CreatedAt = DateTime.UtcNow,
                ExpireAt = DateTime.UtcNow.AddHours(24),
                ResponseBody = response,
                StatusCode = statusCode
            };

            _context.IdempotencyRecords.Add(record);
        }
        else
        {
            record.ResponseBody = response;
            record.StatusCode = statusCode;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<IdempotencyCheckResult> CheckAndReserveAsync(string key, string requestHash)
    {
        var newRecord = new IdempotencyRecord
        {
            Id = Guid.NewGuid(),
            IdempotencyKey = key,
            RequestHash = requestHash,
            CreatedAt = DateTime.UtcNow,
            ExpireAt = DateTime.UtcNow.AddHours(24),
            ResponseBody = string.Empty,
            StatusCode = 0
        };


        // Here I enhanced the logic to use a single database call to check for existing record and insert a new one
        // if it doesn't exist,
        // using SQL MERGE statement. This approach minimizes the chances

        var results = await TryReserveAsync(newRecord);

        var result = results.FirstOrDefault();

        if (result == null)
            return new IdempotencyCheckResult { Status = IdempotencyStatus.InProgress };

        return new IdempotencyCheckResult
        {
            Status = result.GetStatus(newRecord.Id, requestHash),
            Record = result
        };

        #region Switch Case Pattern
        //return result switch
        //{
        //    null => new IdempotencyCheckResult { Status = IdempotencyStatus.InProgress },
        //    _ when result.Id == newRecord.Id => new IdempotencyCheckResult { Status = IdempotencyStatus.New },
        //    _ when result.RequestHash != requestHash => new IdempotencyCheckResult { Status = IdempotencyStatus.Mismatch },
        //    _ when string.IsNullOrEmpty(result.ResponseBody) => new IdempotencyCheckResult { Status = IdempotencyStatus.InProgress },
        //    _ => new IdempotencyCheckResult { Status = IdempotencyStatus.Completed, Record = result }
        //}; 
        #endregion

    }

    private async Task<List<IdempotencyRecord>> TryReserveAsync(IdempotencyRecord newRecord)
    {
        return await _context.IdempotencyRecords
        .FromSqlRaw(@"
            MERGE IdempotencyRecords AS target
            USING (VALUES (@Id, @Key, @Hash, @Body, @Type, @Status, @Created, @Expire)) 
            AS source (Id, IdempotencyKey, RequestHash, ResponseBody, ContentType, StatusCode, CreatedAt, ExpireAt)
            ON target.IdempotencyKey = source.IdempotencyKey
            WHEN MATCHED THEN
            UPDATE SET target.IdempotencyKey = target.IdempotencyKey
            WHEN NOT MATCHED THEN
                INSERT (Id, IdempotencyKey, RequestHash, ResponseBody, ContentType, StatusCode, CreatedAt, ExpireAt)
                VALUES (source.Id, source.IdempotencyKey, source.RequestHash, source.ResponseBody, source.ContentType, source.StatusCode, source.CreatedAt, source.ExpireAt)
            OUTPUT INSERTED.*, $action AS Action;",
            new SqlParameter("@Id", newRecord.Id),
            new SqlParameter("@Key", newRecord.IdempotencyKey),
            new SqlParameter("@Hash", newRecord.RequestHash),
            new SqlParameter("@Body", newRecord.ResponseBody),
            new SqlParameter("@Type", newRecord.ContentType),
            new SqlParameter("@Status", newRecord.StatusCode),
            new SqlParameter("@Created", newRecord.CreatedAt),
            new SqlParameter("@Expire", newRecord.ExpireAt))
        .IgnoreQueryFilters()
        .AsNoTracking()
        .ToListAsync();
    }


    #region Old Implementation that is not good enough because of it hits the database twice, once to check and once to insert
    //public async Task<IdempotencyCheckResult> CheckAndReserveAsync(string key, string requestHash)
    //{
    //    //This line is not good enough ,becuase of it hits the database twice, once to check and once to insert
    //    // TODO : Use Redis
    //    var existing = await _context
    //                              .IdempotencyRecords
    //                              .FirstOrDefaultAsync(x => x.IdempotencyKey == key);

    //    if (existing is not null)
    //    {
    //        if (existing.RequestHash != requestHash)
    //            return new IdempotencyCheckResult { Status = IdempotencyStatus.Mismatch };

    //        if (string.IsNullOrEmpty(existing.ResponseBody))
    //            return new IdempotencyCheckResult { Status = IdempotencyStatus.InProgress };

    //        return new IdempotencyCheckResult { Status = IdempotencyStatus.Completed, Record = existing };
    //    }

    //    var newRecord = new IdempotencyRecord
    //    {
    //        Id = Guid.NewGuid(),
    //        IdempotencyKey = key,
    //        RequestHash = requestHash,
    //        CreatedAt = DateTime.UtcNow,
    //        ExpireAt = DateTime.UtcNow.AddHours(24),
    //        //ExpireAt = DateTime.UtcNow.AddSeconds(5), // causing a duplicate
    //        ResponseBody = string.Empty
    //    };


    //    await TryReserveAsync(newRecord);

    //    return new IdempotencyCheckResult { Status = IdempotencyStatus.New };
    //}
    #endregion

}
