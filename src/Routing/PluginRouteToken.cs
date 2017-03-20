/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2011-2017 Zongsoft Corporation <http://www.zongsoft.com>
 *
 * This file is part of Zongsoft.Plugins.Web.
 *
 * Zongsoft.Plugins.Web is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * Zongsoft.Plugins.Web is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Zongsoft.Plugins.Web; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA
 */

using System;
using System.Collections.Generic;
using System.Threading;

namespace Zongsoft.Web.Routing
{
	internal class RouteToken : IRoute
	{
		#region 成员字段
		private string _url;
		private string _name;
		private string _kind;
		private bool _ignored;
		private object _handler;
		private IDictionary<string, object> _constraints;
		private IDictionary<string, object> _defaults;
		private IDictionary<string, object> _states;

		private Func<RouteToken, object> _routeFactory;
		#endregion

		#region 构造函数
		public RouteToken(string name, string kind = null, string url = null, object handler = null)
		{
			if(string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException(nameof(name));

			_name = name.Trim();
			_kind = string.IsNullOrWhiteSpace(kind) ? string.Empty : kind.Trim();
			_url = string.IsNullOrWhiteSpace(url) ? string.Empty : url.Trim();
			_handler = handler;
		}
		#endregion

		#region 公共属性
		public string Name
		{
			get
			{
				return _name;
			}
		}

		public string Kind
		{
			get
			{
				return _kind;
			}
			set
			{
				_kind = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
			}
		}

		public string Url
		{
			get
			{
				return _url;
			}
			set
			{
				_url = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
			}
		}

		public object Handler
		{
			get
			{
				return _handler;
			}
			set
			{
				_handler = value;
			}
		}

		public bool Ignored
		{
			get
			{
				return _ignored;
			}
			set
			{
				_ignored = value;
			}
		}

		public IDictionary<string, object> Constraints
		{
			get
			{
				if(_constraints == null)
					Interlocked.CompareExchange(ref _constraints, new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase), null);

				return _constraints;
			}
		}

		public IDictionary<string, object> Defaults
		{
			get
			{
				if(_defaults == null)
					Interlocked.CompareExchange(ref _defaults, new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase), null);

				return _defaults;
			}
		}

		public IDictionary<string, object> States
		{
			get
			{
				if(_states == null)
					Interlocked.CompareExchange(ref _states, new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase), null);

				return _states;
			}
		}
		#endregion

		#region 公共方法
		public object ToRoute()
		{
			if(_routeFactory == null)
				return null;

			return _routeFactory(this);
		}
		#endregion

		#region 内部方法
		internal void SetRouteFactory(Func<RouteToken, object> factory)
		{
			if(factory == null)
				throw new ArgumentNullException(nameof(factory));

			_routeFactory = factory;
		}
		#endregion

		#region 重写方法
		public override string ToString()
		{
			if(string.IsNullOrWhiteSpace(_kind))
				return $"{_name}{Environment.NewLine}{_url}";
			else
				return $"[{_kind}]{_name}{Environment.NewLine}{_url}";
		}
		#endregion
	}
}
