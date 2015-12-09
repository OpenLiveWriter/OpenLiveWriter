using System;
using System.Collections;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace BlogServer.Config
{
	public class DiffManager
	{
		private readonly string _configFilePath;
		private ArrayList _configFiles;
		private XmlElement _configRoot;

		public DiffManager(string configFilePath)
		{
			_configFilePath = configFilePath;
			_configFiles = new ArrayList();
			Reload();
		}


		public XmlElement ConfigRoot
		{
			get
			{
				lock (this)
				{
					return _configRoot;
				}
			}
		}

		public bool MaybeReload()
		{
			lock (this)
			{
				foreach (FileCookie fc in _configFiles)
				{
					if (fc.Changed)
					{
						Reload();
						return true;
					}
				}
				return false;
			}
		}
		
		private void Reload()
		{
			_configFiles.Clear();
			_configRoot = Load(_configFilePath).DocumentElement;
		}

		private XmlDocument Load(string path)
		{
			_configFiles.Add(new FileCookie(path));
			XmlDocument doc = new XmlDocument();
			doc.Load(path);
			XmlElement root = doc.DocumentElement;
			if (root.Name == "patch")
			{
				if (!root.HasAttribute("baseConfig"))
					throw new ConfigurationException("Element \"patch\" is missing attribute \"baseConfig\" in file \"" + path + "\"");
				string baseConfig = root.GetAttribute("baseConfig");
				XmlDocument baseConfigDoc = Load(ResolvePath(path, baseConfig));
				Merge(baseConfigDoc, root, path);
				return baseConfigDoc;
			}
			else
				return doc;
		}

		private void Merge(XmlDocument baseConfig, XmlElement patch, string patchPath)
		{
			foreach (XmlNode node in patch.ChildNodes)
			{
				XmlElement el = node as XmlElement;
				if (el == null)
					continue;

				switch (el.Name)
				{
					case "insert":
					case "delete":
					case "replace":
					case "merge":
						break;
					default:
						continue;
				}

				if (!el.HasAttribute("select"))
					throw new ConfigurationException(el.Name + " element is missing required attribute \"select\" in file \"" + patchPath + "\"");
				string select = el.GetAttribute("select");
				
				XmlNodeList targets = baseConfig.SelectNodes(select);
				
				switch (el.Name)
				{
					case "insert":
						string position = el.GetAttribute("position");
						if (position == null || position.Length == 0)
							position = Position.lastChild.ToString();
						foreach (XmlElement element in targets)
							AddElements(element, el.ChildNodes, (Position) Enum.Parse(typeof(Position), position, false));
						break;
					case "delete":
						foreach (XmlElement element in targets)
							DeleteElement(element);
						break;
					case "replace":
						foreach (XmlElement element in targets)
							ReplaceElement(element, el.ChildNodes);
						break;
					case "merge":
						foreach (XmlElement element in targets)
							MergeElement(element, el);
						break;
				}
			}
		}
		
		private void ReplaceElement(XmlElement baseElement, XmlNodeList patchNodes)
		{
			AddElements(baseElement, patchNodes, Position.before);
			DeleteElement(baseElement);
		}

		private static void AddElements(XmlElement baseElement, XmlNodeList patchNodes, Position position)
		{
			if (position == Position.before)
			{
				foreach (XmlNode node in patchNodes)
				{
					XmlNode newEl = baseElement.OwnerDocument.ReadNode(new XmlNodeReader(node));
					baseElement.ParentNode.InsertBefore(newEl, baseElement);
				}
			}
			else if (position == Position.after)
			{
				for (int i = 0; i < patchNodes.Count; i++)
				{
					XmlNode newEl = baseElement.OwnerDocument.ReadNode(new XmlNodeReader(patchNodes[patchNodes.Count - 1 - i]));
					baseElement.ParentNode.InsertAfter(newEl, baseElement);
				}
			}
			else if (position == Position.lastChild)
			{
				foreach (XmlNode node in patchNodes)
				{
					XmlNode newEl = baseElement.OwnerDocument.ReadNode(new XmlNodeReader(node));
					baseElement.AppendChild(newEl);
				}
			}
			else if (position == Position.firstChild)
			{
				foreach (XmlNode node in patchNodes)
				{
					XmlNode newEl = baseElement.OwnerDocument.ReadNode(new XmlNodeReader(node));
					baseElement.InsertAfter(newEl, null);
				}
			}
		}

		private enum Position
		{
			before,
			after,
			firstChild,
			lastChild
		}

		private void MergeElement(XmlElement baseElement, XmlElement patchElement)
		{
			XmlElement newEl = (XmlElement) baseElement.OwnerDocument.ReadNode(new XmlNodeReader(patchElement));
			newEl.RemoveAttribute("select");
			if (newEl.ChildNodes.Count > 0)
				AddElements(baseElement, newEl.ChildNodes, Position.lastChild);
			foreach (XmlAttribute attr in newEl.Attributes)
				baseElement.SetAttribute(attr.Name, attr.NamespaceURI, attr.Value);
		}

		private void DeleteElement(XmlElement element)
		{
			element.ParentNode.RemoveChild(element);
		}

		private static string ResolvePath(string basePath, string relativePath)
		{
			if (Path.IsPathRooted(relativePath))
				return relativePath;
			string path = Path.Combine(Path.GetDirectoryName(basePath), relativePath);
			if (File.Exists(path))
				return path;
			path = Path.Combine(
				Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName),
				relativePath);
			if (File.Exists(path))
				return path;
			
			throw new ConfigurationException("Couldn't resolve the file \"" + relativePath + "\", which was referenced by \"" +
			                                 basePath + "\"");
		}
		
		private class FileCookie
		{
			private readonly string _filePath;
			private readonly DateTime _timestamp;

			public FileCookie(string filePath)
			{
				_filePath = filePath;
				_timestamp = File.GetLastWriteTimeUtc(_filePath);
			}
			
			public bool Changed
			{
				get { return _timestamp != File.GetLastWriteTimeUtc(_filePath); }
			}
		}
	}
}
