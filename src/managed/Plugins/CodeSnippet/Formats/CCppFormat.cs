using System;

namespace CodeSnippet.Formats
{
	public class CCppFormat : CLikeFormat
	{
		protected override string Keywords
		{
			get
			{
				return "asm auto bool break case catch char class const const_cast continue default delete do double dynamic_cast else enum explicit export extern FALSE float for friend goto if inline int long mutable namespace new operator private protected public register reinterpret_cast return short signed sizeof static static_cast struct switch template this throw TRUE try typedef typeid typename union unsigned using virtual void volatile wchar_t while";
			}
		}

		protected override string Preprocessors
		{
			get
			{
				return "#define #elif #else #endif #error #if #ifdef #ifndef #import #include #line #pragma #undef #using alloc_text auto_inline bss_seg check_stack code_seg comment component conform1 const_seg data_seg deprecated fenv_access float_control fp_contract function hdrstop include_alias init_seg1 inline_depth inline_recursion intrinsic make_public managed message omp once optimize pack pointers_to_members1 pop_macro push_macro region endregion runtime_checks section setlocale strict_gs_check unmanaged vtordisp1 warning";
			}
		}

		public CCppFormat()
		{
		}
	}
}