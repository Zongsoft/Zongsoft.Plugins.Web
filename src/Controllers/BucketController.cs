/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2015 Zongsoft Corporation <http://www.zongsoft.com>
 *
 * This file is part of Zongsoft.Web.Plugins.
 *
 * Zongsoft.Web.Plugins is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * Zongsoft.Web.Plugins is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 * Lesser General Public License for more details.
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Zongsoft.Web.Plugins; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA
 */

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using Zongsoft.IO;
using Zongsoft.Common;

namespace Zongsoft.Web.Plugins.Controllers
{
	[Obsolete]
	public class BucketController : ApiController
	{
		#region 成员字段
		private Zongsoft.IO.IStorageBucket _bucket;
		private Zongsoft.Common.ISequence _sequence;
		#endregion

		#region 公共属性
		public Zongsoft.IO.IStorageBucket Bucket
		{
			get
			{
				if(_bucket == null)
					throw new InvalidOperationException("The value of 'Bucket' property is null.");

				return _bucket;
			}
			set
			{
				if(value == null)
					throw new ArgumentNullException();

				_bucket = value;
			}
		}

		public Zongsoft.Common.ISequence Sequence
		{
			get
			{
				if(_bucket == null)
					throw new InvalidOperationException("The value of 'Sequence' property is null.");

				return _sequence;
			}
			set
			{
				if(value == null)
					throw new ArgumentNullException();

				_sequence = value;
			}
		}
		#endregion

		#region 公共方法
		/// <summary>
		/// 获取指定文件容器的详细信息。
		/// </summary>
		/// <param name="id">指定的文件容器编号。</param>
		/// <returns>返回的文件容器信息的实体对象。</returns>
		public StorageBucketInfo Get(int id)
		{
			return this.Bucket.GetInfo(id);
		}

		/// <summary>
		/// 获取指定文件容器的存储路径。
		/// </summary>
		/// <param name="id">指定的文件容器编号。</param>
		/// <returns>返回指定文件容器的存储路径。</returns>
		[HttpGet]
		public string Path(int id)
		{
			return this.Bucket.GetPath(id);
		}

		/// <summary>
		/// 新增一个文件容器。
		/// </summary>
		/// <param name="bucket">新增的文件容器实体对象。</param>
		public int Post(StorageBucketInfo bucket)
		{
			if(bucket == null)
				throw new ArgumentNullException("bucket");

			if(bucket.BucketId < 1)
				bucket.BucketId = (int)this.Sequence.GetSequenceNumber("Zongsoft.IO.StorageBucket.Id");

			this.Bucket.Create(bucket.BucketId, bucket.Name, bucket.Title, bucket.Path);

			//返回新增文件容器的编号
			return (int)bucket.BucketId;
		}

		/// <summary>
		/// 删除指定编号的文件容器。
		/// </summary>
		/// <param name="id">指定要删除的文件容器编号。</param>
		public bool Delete(int id)
		{
			return this.Bucket.Delete(id);
		}

		/// <summary>
		/// 修改指定的文件容器信息。
		/// </summary>
		/// <param name="bucket">指定要修改的文件容器信息实体对象。</param>
		public void Put(StorageBucketInfo bucket)
		{
			if(bucket == null)
				throw new ArgumentNullException("bucket");

			if(bucket.BucketId < 1)
				throw new ArgumentOutOfRangeException("Bucket.BucketId");

			this.Bucket.Modify(bucket.BucketId, bucket.Name, bucket.Title, bucket.Path, bucket.ModifiedTime);
		}

		/// <summary>
		/// 获取指定文件容器总数或指定文件容器内的文件总数。
		/// </summary>
		/// <param name="id">指定的文件容器编号。</param>
		/// <returns>如果未指定文件容器编号，即<paramref name="id"/>参数是否为空(null)，则返回的文件容器总数，否则返回指定编号的文件容器内的文件总数。</returns>
		[HttpGet]
		public int Count(int? id)
		{
			if(id.HasValue)
				return this.Bucket.GetFileCount(id.Value);
			else
				return this.Bucket.GetBucketCount();
		}
		#endregion
	}
}
