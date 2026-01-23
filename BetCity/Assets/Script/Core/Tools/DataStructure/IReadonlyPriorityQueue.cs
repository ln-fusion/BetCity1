using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BetCity.Core.Tools
{
    /// <summary>
    /// 只读优先队列接口（仅暴露查询能力，无修改操作）
    /// </summary>
    /// <typeparam name="T">队列元素类型</typeparam>
    /// <typeparam name="TPriority">优先级类型（需实现 IComparable）</typeparam>
    public interface IReadOnlyPriorityQueue<T, TPriority> : IEnumerable<T>
        where TPriority : IComparable<TPriority>
    {
        /// <summary>
        /// 队列中元素的数量
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 队列是否为空
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// 查看队首元素（优先级最高，不移除）
        /// </summary>
        /// <returns>优先级最高的元素</returns>
        /// <exception cref="InvalidOperationException">队列为空时抛出</exception>
        T Peek();

        /// <summary>
        /// 检查指定元素是否存在于队列中
        /// </summary>
        /// <param name="element">要检查的元素</param>
        /// <returns>是否存在</returns>
        bool Contains(T element);
    }
}
