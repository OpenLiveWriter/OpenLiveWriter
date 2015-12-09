// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#pragma once

class SafeBuffer
{
public:
	void *p;

	SafeBuffer(size_t size)
	{
		p = malloc(size);
	}

	~SafeBuffer(void)
	{
		free(p);
	}
};
