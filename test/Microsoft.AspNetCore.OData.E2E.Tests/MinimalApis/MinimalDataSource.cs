//-----------------------------------------------------------------------------
// <copyright file="MinimalDataSource.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.OData.E2E.Tests.MinimalApis;

public interface IMiniTodoTaskRepository
{
    IEnumerable<MiniTodo> GetTodos();

    IEnumerable<MiniTask> GetTasks();

    MiniTodo GetTodo(int id);

    MiniTask GetTask(int id);
}

public class MiniTodoTaskInMemoryRepository : IMiniTodoTaskRepository
{
    private static IList<MiniTodo> _todos;
    private static IList<MiniTask> _tasks;

    public IEnumerable<MiniTodo> GetTodos() => _todos;

    public IEnumerable<MiniTask> GetTasks() => _tasks;

    public MiniTodo GetTodo(int id) => _todos.FirstOrDefault(t => t.Id == id);

    public MiniTask GetTask(int id) => _tasks.FirstOrDefault(t => t.Id == id);

    #region TodoTasks
    static MiniTodoTaskInMemoryRepository()
    {
        _todos = new List<MiniTodo>
        {
            new MiniTodo
            {
                Id = 1, Owner = "Peter", Title = "Cooking", IsDone = false,
                Tasks =
                [
                    new MiniTask { Id = 11, Created = new DateOnly(2021, 4, 22), Description = "Boil Rice", IsComplete = true, Priority = 1 },
                    new MiniTask { Id = 12, Created = new DateOnly(2022, 4, 22), Description = "Cook Potate", IsComplete = false, Priority = 1 },
                    new MiniTask { Id = 13, Created = new DateOnly(2021, 4, 22), Description = "Cook Pizza", IsComplete = false, Priority = 2 },
                ]
            },
            new MiniTodo
            {
                Id = 2, Owner = "Wu", Title = "English Practice", IsDone = true,
                Tasks =
                [
                    new MiniTask { Id = 21, Created = new DateOnly(2024, 2, 11), Description = "Read English book", IsComplete = true, Priority = 1 },
                    new MiniTask { Id = 22, Created = new DateOnly(2022, 3, 4), Description = "Watch video", IsComplete = false, Priority = 2 },
                ]
            },
            new MiniTodo
            {
                Id = 3, Owner = "John", Title = "Shopping", IsDone = true,
                Tasks =
                [
                    new MiniTask { Id = 31, Created = new DateOnly(2022, 2, 11), Description = "Buy bread", IsComplete = false, Priority = 3 },
                    new MiniTask { Id = 32, Created = new DateOnly(2023, 12, 14), Description = "Buy washing machine", IsComplete = true, Priority = 2 },
                ]
            },
            new MiniTodo
            {
                Id = 4, Owner = "Sam", Title = "Clean House", IsDone = false,
                Tasks =
                [
                    new MiniTask { Id = 41, Created = new DateOnly(2025, 2, 11), Description = "Clean carpet", IsComplete = false, Priority = 2 },
                    new MiniTask { Id = 42, Created = new DateOnly(2025, 12, 14), Description = "Clean bathroom", IsComplete = true, Priority = 1 },
                ]
            }
        };

        List<MiniTask> tasks = new List<MiniTask>();
        foreach (var todo in _todos)
        {
            tasks.AddRange(todo.Tasks);
        }

        _tasks = tasks;
    }
    #endregion
}
