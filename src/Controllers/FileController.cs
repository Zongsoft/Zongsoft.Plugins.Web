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
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;

using Zongsoft.IO;
using Zongsoft.Common;

namespace Zongsoft.Web.Plugins.Controllers
{
	[Obsolete]
	public class FileController : ApiController
	{
		#region 成员字段
		private Zongsoft.IO.IStorageFile _file;
		private Zongsoft.IO.IStorageBucket _bucket;
		#endregion

		#region 构造函数
		public FileController()
		{
		}

		public FileController(Zongsoft.Services.IServiceProvider serviceProvider)
		{
			if(serviceProvider == null)
				throw new ArgumentNullException("serviceProvider");

			_file = serviceProvider.Resolve<IStorageFile>();
			_bucket = serviceProvider.Resolve<IStorageBucket>();
		}
		#endregion

		#region 公共属性
		public Zongsoft.IO.IStorageFile File
		{
			get
			{
				if(_file == null)
					throw new InvalidOperationException("The value of 'File' property is null.");

				return _file;
			}
			set
			{
				if(value == null)
					throw new ArgumentNullException();

				_file = value;
			}
		}

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
		#endregion

		#region 公共方法
		/// <summary>
		/// 下载指定编号的文件。
		/// </summary>
		/// <param name="id">指定要下载的文件编号。</param>
		public HttpResponseMessage Get(Guid id)
		{
			StorageFileInfo info;

			var stream = this.File.Open(id, out info);

			if(info == null)
				throw new HttpResponseException(HttpStatusCode.NotFound);

			//创建当前文件的流内容
			var content = new StreamContent(stream);

			//设置返回的内容头信息
			content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");

			if(!string.IsNullOrWhiteSpace(info.Type))
				content.Headers.ContentType = new MediaTypeHeaderValue(info.Type);
			else
				content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

			if(!string.IsNullOrWhiteSpace(info.Name))
				content.Headers.ContentDisposition.FileName = info.Name;

			content.Headers.LastModified = info.ModifiedTime.HasValue ? info.ModifiedTime.Value : info.CreatedTime;

			return new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = content,
			};
		}

		/// <summary>
		/// 获取指定编号的文件描述信息。
		/// </summary>
		/// <param name="id">指定要获取的文件编号。</param>
		/// <returns>返回的指定的文件详细信息。</returns>
		[HttpGet]
		public StorageFileInfo Info(Guid id)
		{
			return this.File.GetInfo(id);
		}

		/// <summary>
		/// 获取指定文件的存储路径。
		/// </summary>
		/// <param name="id">指定的文件编号。</param>
		/// <returns>返回指定文件的存储路径。</returns>
		[HttpGet]
		public string Path(Guid id)
		{
			return this.File.GetPath(id);
		}

		/// <summary>
		/// 新增一个文件或多个文件。
		/// </summary>
		/// <param name="id">指定新增文件所属的容器编号。</param>
		/// <returns>返回新增文件的<see cref="Models.StorageFile"/>描述信息实体对象集。</returns>
		public async Task<ICollection<StorageFileInfo>> Post(int id)
		{
			if(id < 1)
				throw new ArgumentOutOfRangeException("id(BucketId)");

			//检测请求的内容是否为Multipart类型
			if(!this.Request.Content.IsMimeMultipartContent("form-data"))
				throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

			var bucketInfo = this.Bucket.GetInfo(id);

			if(bucketInfo == null)
				throw new HttpResponseException(HttpStatusCode.NotFound);

			//创建多段表单信息的文件流操作的供应程序
			var provider = new MultipartStorageFileStreamProvider(id, ResolveBucketPath(bucketInfo.Path, DateTime.Now));

			//从当前请求内容读取多段信息并写入文件中
			return await this.Request.Content.ReadAsMultipartAsync(provider).ContinueWith(t =>
			{
				var pro = t.Result;

				foreach(var fileInfo in pro.FileData)
				{
					if(pro.FormData.Count > 0)
					{
						for(int i = 0; i < pro.FormData.Count; i++ )
							fileInfo.ExtendedProperties.Add(pro.FormData.GetKey(i), pro.FormData[i]);
					}

					this.File.Create(fileInfo, null);
				}

				//返回新增的文件信息实体集
				return pro.FileData;
			});
		}

		/// <summary>
		/// 删除指定编号的文件。
		/// </summary>
		/// <param name="id">指定要删除的文件编号。</param>
		public bool Delete(Guid id)
		{
			return this.File.Delete(id);
		}

		/// <summary>
		/// 修改指定编号的文件内容。
		/// </summary>
		/// <param name="id">指定要修改的文件编号。</param>
		public void Put(Guid id)
		{
			throw new NotImplementedException();
		}
		#endregion

		internal class MultipartStorageFileStreamProvider : MultipartStreamProvider
		{
			#region 成员字段
			private int _bucketId;
			private string _bucketPath;
			private Collection<StorageFileInfo> _fileData;
			private System.Collections.Specialized.NameValueCollection _formData;
			private Collection<bool> _isFormData;
			#endregion

			#region 构造函数
			public MultipartStorageFileStreamProvider(int bucketId, string bucketPath)
			{
				if(bucketId < 1)
					throw new ArgumentOutOfRangeException("bucketId");

				if(string.IsNullOrWhiteSpace(bucketPath))
					throw new ArgumentNullException("bucketPath");

				_bucketId = bucketId;
				_bucketPath = bucketPath;
				_fileData = new Collection<StorageFileInfo>();
				_isFormData = new Collection<bool>();
				_formData = new System.Collections.Specialized.NameValueCollection();
			}
			#endregion

			#region 公共属性
			public int BucketId
			{
				get
				{
					return _bucketId;
				}
			}

			public string BucketPath
			{
				get
				{
					return _bucketPath;
				}
			}

			public Collection<StorageFileInfo> FileData
			{
				get
				{
					return _fileData;
				}
			}

			public System.Collections.Specialized.NameValueCollection FormData
			{
				get
				{
					return _formData;
				}
			}
			#endregion

			#region 重写方法
			public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
			{
				if(parent == null)
					throw new ArgumentNullException("parent");

				if(headers == null)
					throw new ArgumentNullException("headers");

				if(string.IsNullOrEmpty(headers.ContentDisposition.FileName))
				{
					_isFormData.Add(true);
					return new MemoryStream();
				}

				var contentType = headers.ContentType == null ? string.Empty : headers.ContentType.MediaType;

				if(string.IsNullOrWhiteSpace(contentType))
					contentType = System.Web.MimeMapping.GetMimeMapping(headers.ContentDisposition.FileName);

				_isFormData.Add(false);

				//创建一个文件信息实体对象
				var fileInfo = new StorageFileInfo(_bucketId)
				{
					Name = Zongsoft.Common.StringExtension.RemoveCharacters(headers.ContentDisposition.FileName, System.IO.Path.GetInvalidFileNameChars()).Trim('"', '\''),
					Type = contentType,
					Size = headers.ContentDisposition.Size.HasValue ? headers.ContentDisposition.Size.Value : -1,
				};

				fileInfo.Path = this.GetFilePath(fileInfo);

				try
				{
					//将文件信息对象加入到集合中
					_fileData.Add(fileInfo);

					if(!FileSystem.Directory.Exists(_bucketPath))
						FileSystem.Directory.Create(_bucketPath);

					return FileSystem.File.Open(fileInfo.Path, FileMode.CreateNew, FileAccess.Write);
				}
				catch
				{
					_fileData.Remove(fileInfo);
					throw;
				}
			}

			public override Task ExecutePostProcessingAsync()
			{
				return Task.Run(() =>
				{
					int index = 0;

					foreach(var content in this.Contents)
					{
						if(_isFormData[index++])
						{
							string fieldName = this.UnquoteToken(content.Headers.ContentDisposition.Name) ?? string.Empty;
							_formData.Add(fieldName, content.ReadAsStringAsync().Result);
						}
					}
				});
			}
			#endregion

			#region 私有方法
			private string GetFilePath(StorageFileInfo fileInfo)
			{
				//返回当前文件的完整虚拟路径
				return Zongsoft.IO.Path.Combine(_bucketPath, string.Format("[{0}]{1:n}{2}", fileInfo.BucketId, fileInfo.FileId, System.IO.Path.GetExtension(fileInfo.Name).ToLowerInvariant()));
			}

			private string UnquoteToken(string token)
			{
				if(string.IsNullOrWhiteSpace(token))
					return token;

				if(token.StartsWith("\"", StringComparison.Ordinal) && token.EndsWith("\"", StringComparison.Ordinal) && token.Length > 1)
					return token.Substring(1, token.Length - 2);

				return token;
			}
			#endregion
		}

		private static string ResolveBucketPath(string path, DateTime timestamp)
		{
			if(!string.IsNullOrWhiteSpace(path))
			{
				return path.Replace("$(year)", timestamp.Year.ToString("0000"))
						   .Replace("$(month)", timestamp.Month.ToString("00"))
						   .Replace("$(day)", timestamp.Day.ToString("00"));
			}

			return path;
		}
	}
}
