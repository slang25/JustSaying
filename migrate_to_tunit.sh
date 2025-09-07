#!/bin/bash

# TUnit Migration Script
# This script converts xUnit tests to TUnit format

TEST_DIR="/Users/stuart/git/github/JustSaying/tests/JustSaying.UnitTests"

echo "Starting TUnit migration..."

# Find all .cs files in the test directory
find "$TEST_DIR" -name "*.cs" -type f | while read -r file; do
    echo "Processing $file"
    
    # Replace attributes
    sed -i '' 's/\[Fact\]/[Test]/g' "$file"
    sed -i '' 's/\[Theory\]/[Test]/g' "$file"
    sed -i '' 's/\[InlineData\]/[Arguments]/g' "$file"
    
    # Convert method signatures to async Task (but be careful with void methods)
    # First replace public void with public async Task for test methods that have [Test]
    sed -i '' '/\[Test\]/{
        N
        s/public void \([^(]*\)(/public async Task \1(/
    }' "$file"
    
    # Replace Assert calls with TUnit fluent syntax
    sed -i '' 's/Assert\.Equal(\([^,]*\), \([^)]*\));/await Assert.That(\2).IsEqualTo(\1);/g' "$file"
    sed -i '' 's/Assert\.NotEqual(\([^,]*\), \([^)]*\));/await Assert.That(\2).IsNotEqualTo(\1);/g' "$file"
    sed -i '' 's/Assert\.True(\([^)]*\));/await Assert.That(\1).IsTrue();/g' "$file"
    sed -i '' 's/Assert\.False(\([^)]*\));/await Assert.That(\1).IsFalse();/g' "$file"
    sed -i '' 's/Assert\.Null(\([^)]*\));/await Assert.That(\1).IsNull();/g' "$file"
    sed -i '' 's/Assert\.NotNull(\([^)]*\));/await Assert.That(\1).IsNotNull();/g' "$file"
    
    # Handle Assert.Throws - this is more complex, simple replacement for now
    sed -i '' 's/Assert\.Throws<\([^>]*\)>(\([^)]*\));/await Assert.That(\2).Throws<\1>();/g' "$file"
    sed -i '' 's/await Assert\.ThrowsAsync<\([^>]*\)>(\([^)]*\));/await Assert.That(\2).ThrowsAsync<\1>();/g' "$file"
    sed -i '' 's/await Assert\.ThrowsAnyAsync<\([^>]*\)>(\([^)]*\));/await Assert.That(\2).ThrowsAnyAsync<\1>();/g' "$file"
    
done

echo "Migration completed!"
echo "Files processed. Please review the changes and build the project."
