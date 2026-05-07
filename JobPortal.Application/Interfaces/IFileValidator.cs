using System;

namespace JobPortal.Application.Interfaces;

public interface IFileValidator
{
    public void Validate(string fileName, long fileSizeBytes);
}
