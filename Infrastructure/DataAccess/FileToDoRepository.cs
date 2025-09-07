using System.Text.Json;

public class FileToDoRepository : IToDoRepository
{
    private readonly string _baseDirectory;
    private readonly string _indexFile;

    public FileToDoRepository(string baseDirectory)
    {
        if (!Directory.Exists(baseDirectory))
        {
            Directory.CreateDirectory(baseDirectory);
        }
        _baseDirectory = baseDirectory;

        _indexFile = Path.Combine(_baseDirectory, "index.json");
        if (!File.Exists(_indexFile))
        {
            CreateFileIndex();
        }
    }

    #region Public Methods Implementation

    public async Task<ToDoItem?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return await GetByGuid(id, cancellationToken);
    }

    public async Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await GetAllByUserId(userId, cancellationToken);
    }

    public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await GetActiveByUserId(userId, cancellationToken);
    }

    public async Task AddAsync(ToDoItem item, CancellationToken cancellationToken)
    {
        await Add(item, cancellationToken);
    }

    public async Task UpdateAsync(ToDoItem item, CancellationToken cancellationToken)
    {
        await Update(item, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await Delete(id, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken cancellationToken)
    {
        return await ExistsByName(userId, name, cancellationToken);
    }

    public async Task<int> CountActiveAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await CountActive(userId, cancellationToken);
    }

    public async Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken cancellationToken)
    {
        return await Find(userId, predicate, cancellationToken);
    }

    #endregion

    #region Private Helper Methods

    private async Task WriteFileIndex(List<Index> indexes)
    {
        await File.WriteAllTextAsync(_indexFile, JsonSerializer.Serialize<List<Index>>(indexes, new JsonSerializerOptions { WriteIndented = true }), cancellationToken: CancellationToken.None);
    }

    private async Task<List<Index>> ReadFileIndex()
    {
        if (!File.Exists(_indexFile))
        {
            await CreateFileIndex();
        }
        return JsonSerializer.Deserialize<List<Index>>(File.ReadAllText(_indexFile)) ?? new List<Index>();
    }

    private async Task CreateFileIndex()
    {
        var indexes = new List<Index>();
        foreach (var folder in Directory.EnumerateDirectories(_baseDirectory))
        {
            foreach (var file in Directory.EnumerateFiles(folder, "*.json"))
            {
                try
                {
                    var item = JsonSerializer.Deserialize<ToDoItem>(File.ReadAllText(file));
                    if (item != null)
                    {
                        indexes.Add(new Index(item.ToDoUser.UserId, item.Id));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при обработке файла {file}: {ex.Message}");
                }
            }
        }
        await WriteFileIndex(indexes);
    }

    private async Task<ToDoItem?> GetByGuid(Guid id, CancellationToken cancellationToken)
    {
        var indexRecords = await ReadFileIndex();
        var indexRecord = indexRecords.FirstOrDefault(record => record.ItemId == id);


        var filePath = Path.Combine(_baseDirectory, indexRecord.UserId.ToString(), $"{id}.json");
        try
        {
            return JsonSerializer.Deserialize<ToDoItem>(File.ReadAllText(filePath));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при чтении файла {filePath}: {ex.Message}");
            return null;
        }
    }

    private async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken cancellationToken)
    {
        var userFolder = Path.Combine(_baseDirectory, userId.ToString());
        if (!Directory.Exists(userFolder))
        {
            return new List<ToDoItem>().AsReadOnly();
        }

        return Directory.EnumerateFiles(userFolder)
            .Where(file => Path.GetExtension(file) == ".json")
            .Select(file =>
            {
                try
                {
                    return JsonSerializer.Deserialize<ToDoItem>(File.ReadAllText(file));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при обработке файла {file}: {ex.Message}");
                    return null;
                }
            })
            .Where(item => item != null)
            .ToList()
            .AsReadOnly();
    }

    private async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken cancellationToken)
    {
        var allItems = await GetAllByUserId(userId, cancellationToken);
        return allItems.Where(i => i.State == ToDoItemState.Active).ToList().AsReadOnly();
    }

    private async Task Add(ToDoItem item, CancellationToken cancellationToken)
    {
        var index = await ReadFileIndex();
        var userFolder = Path.Combine(_baseDirectory, item.ToDoUser.UserId.ToString());
        if (!Directory.Exists(userFolder))
        {
            Directory.CreateDirectory(userFolder);
        }

        var filePath = Path.Combine(userFolder, $"{item.Id}.json");
        await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(item, new JsonSerializerOptions { WriteIndented = true }), cancellationToken);

        index.Add(new Index(item.ToDoUser.UserId, item.Id));
        await WriteFileIndex(index);
    }

    private async Task Update(ToDoItem item, CancellationToken cancellationToken)
    {
        var filePath = Path.Combine(_baseDirectory, item.ToDoUser.UserId.ToString(), $"{item.Id}.json");
        await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(item, new JsonSerializerOptions { WriteIndented = true }), cancellationToken);
    }

    private async Task Delete(Guid id, CancellationToken cancellationToken)
    {
        var index = await ReadFileIndex();
        var indexRecord = index.FirstOrDefault(record => record.ItemId == id);
        if (indexRecord.ItemId != id)
        {
            return;
        }

        index.Remove(indexRecord);
        await WriteFileIndex(index);

        var filePath = Path.Combine(_baseDirectory, indexRecord.UserId.ToString(), $"{id}.json");
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    private async Task<bool> ExistsByName(Guid userId, string name, CancellationToken cancellationToken)
    {
        var items = await GetAllByUserId(userId, cancellationToken);
        return items.Any(i => i.Name == name);
    }

    private async Task<int> CountActive(Guid userId, CancellationToken cancellationToken)
    {
        var items = await GetAllByUserId(userId, cancellationToken);
        return items.Count(i => i.State == ToDoItemState.Active);
    }

    private async Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken cancellationToken)
    {
        var items = await GetAllByUserId(userId, cancellationToken);
        return items.Where(predicate).ToList().AsReadOnly();
    }

    #endregion
}

internal struct Index
{
    public Guid UserId { get; set; }
    public Guid ItemId { get; set; }

    public Index(Guid userId, Guid itemId)
    {
        UserId = userId;
        ItemId = itemId;
    }
}