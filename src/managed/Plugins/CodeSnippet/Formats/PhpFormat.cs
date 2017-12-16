using System;

namespace CodeSnippet.Formats
{
	public class PhpFormat : CLikeFormat
	{
		protected override string CommentRegex
		{
			get
			{
				return string.Concat(base.CommentRegex, "|#.*?(?=\\r|\\n)");
			}
		}

		protected override string Keywords
		{
			get
			{
				return "and or xor __file__ __line__ array as break case cfunction class const continue declare default die do echo else elseif empty enddeclare endfor endforeach endif endswitch endwhile eval exit extends for foreach function global if include include_once isset list new old_function print require require_once return static switch unset use var while __function__ __class__ php_version php_os default_include_path pear_install_dir pear_extension_dir php_extension_dir php_bindir php_libdir php_datadir php_sysconfdir php_localstatedir php_config_file_path php_output_handler_start php_output_handler_cont php_output_handler_end e_error e_warning e_parse e_notice e_core_error e_core_warning e_compile_error e_compile_warning e_user_error e_user_warning e_user_notice e_all true false bool boolean int integer float double real string array object resource null class extends parent stdclass directory __sleep __wakeup interface implements abstract public protected private";
			}
		}

		public PhpFormat()
		{
		}
	}
}