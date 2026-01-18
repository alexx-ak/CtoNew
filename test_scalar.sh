#!/bin/bash

echo "Testing Scalar integration for VoxBox API..."

cd /home/engine/project/backend/src/VoxBox.Api

# Check if the build succeeds
echo "Building the project..."
dotnet build --nologo --verbosity quiet

if [ $? -ne 0 ]; then
    echo "Build failed!"
    exit 1
fi

echo "Build successful!"

# Check if Scalar.AspNetCore package is referenced
echo "Checking for Scalar.AspNetCore package..."
grep -q "Scalar.AspNetCore" VoxBox.Api.csproj

if [ $? -eq 0 ]; then
    echo "✓ Scalar.AspNetCore package found"
else
    echo "✗ Scalar.AspNetCore package not found"
    exit 1
fi

# Check if Scalar is configured in Program.cs
echo "Checking Scalar configuration in Program.cs..."
grep -q "MapScalarApiReference" Program.cs

if [ $? -eq 0 ]; then
    echo "✓ Scalar mapping found in Program.cs"
else
    echo "✗ Scalar mapping not found in Program.cs"
    exit 1
fi

# Check if Scalar using directive is present
echo "Checking Scalar using directive..."
grep -q "using Scalar.AspNetCore" Program.cs

if [ $? -eq 0 ]; then
    echo "✓ Scalar using directive found"
else
    echo "✗ Scalar using directive not found"
    exit 1
fi

# Check if redirect to scalar is configured
echo "Checking scalar redirect configuration..."
grep -q "Redirect.*scalar" Program.cs

if [ $? -eq 0 ]; then
    echo "✓ Scalar redirect configuration found"
else
    echo "✗ Scalar redirect configuration not found"
    exit 1
fi

echo ""
echo "✅ All Scalar integration checks passed!"
echo ""
echo "To test the application:"
echo "1. Run: cd /home/engine/project/backend/src/VoxBox.Api && dotnet run --urls='http://localhost:5000' --environment='Development'"
echo "2. Open browser to: http://localhost:5000"
echo "3. You should be automatically redirected to: http://localhost:5000/scalar/v1"