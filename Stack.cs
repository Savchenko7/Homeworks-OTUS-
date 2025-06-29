using System;
using System.Collections.Generic;

public class StackItem
{
    internal string Value { get; set; }
    internal StackItem Previous { get; set; }
}

public class Stack
{
    private StackItem _top; // ссылка на вершину стека

    // Конструктор позволяет передавать любое число строковых аргументов
    public Stack(params string[] items)
    {
        foreach (var item in items)
        {
            Add(item);
        }
    }

    // Добавляет строку в конец стека
    public void Add(string value)
    {
        var newItem = new StackItem { Value = value };

        if (_top == null)
        {
            _top = newItem;
        }
        else
        {
            newItem.Previous = _top;
            _top = newItem;
        }
    }

    // Возвращает и удаляет верхний элемент стека
    public string Pop()
    {
        if (_top == null)
        {
            throw new InvalidOperationException("Стек пустой");
        }

        var currentTop = _top.Value;
        _top = _top.Previous;
        return currentTop;
    }

    // Количество элементов в стеке
    public int Size
    {
        get
        {
            var count = 0;
            var current = _top;

            while (current != null)
            {
                count++;
                current = current.Previous;
            }

            return count;
        }
    }

    // Верхний элемент стека
    public string Top => _top?.Value;

    // Расширяющий метод для слияния двух стеков
    public void Merge(Stack otherStack)
    {
        while (otherStack._top != null)
        {
            this.Add(otherStack.Pop());
        }
    }

    // Статический метод конкатенации нескольких стеков
    public static Stack Concat(params Stack[] stacks)
    {
        var mergedStack = new Stack();

        foreach (var stack in stacks)
        {
            while (stack._top != null)
            {
                mergedStack.Add(stack.Pop());
            }
        }

        return mergedStack;
    }
}

// Класс расширений
public static class StackExtensions
{
    // Методы расширения автоматически становятся доступными в классе Stack
    public static void Merge(this Stack targetStack, Stack sourceStack)
    {
        targetStack.Merge(sourceStack);
    }
}