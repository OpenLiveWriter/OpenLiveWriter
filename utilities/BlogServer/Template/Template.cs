using System;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using DynamicTemplate.Compiler;

namespace DynamicTemplate
{
    public class Template
    {
    	private TemplateOperation _template;

    	private Template(TemplateOperation template)
    	{
    		_template = template;
    	}

    	public static Template Compile(string template, params ArgumentDescription[] args)
    	{
    		return new Template(BodyParser.Parse(template, args));
    	}
    	
    	public string Execute(params object[] parameters)
    	{
    		return _template(parameters);
    	}
    }
}
