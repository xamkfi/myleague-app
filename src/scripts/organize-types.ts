import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const GENERATED_FILE = path.join(__dirname, '../frontend/src/types/generated/generated-types.ts');
const OUTPUT_DIR = path.join(__dirname, '../frontend/src/types');

interface TypeDefinition {
  name: string;
  content: string;
  category: 'common' | 'floorball' | 'admin';
  dependencies: string[]; // Type names this type depends on
}

function categorizeType(name: string): 'common' | 'floorball' | 'admin' {
  const lowerName = name.toLowerCase();
  
  // Check for floorball-specific keywords
  if (lowerName.includes('floorball') || 
      lowerName.includes('match') || 
      lowerName.includes('team') ||
      lowerName.includes('season') ||
      lowerName.includes('player') ||
      lowerName.includes('goal') ||
      lowerName.includes('penalty') ||
      lowerName.includes('save') ||
      lowerName.includes('referee')) {
    return 'floorball';
  }
  
  // Check for admin/user-specific keywords
  if (lowerName.includes('admin') || 
      lowerName.includes('user') ||
      lowerName.includes('role') ||
      lowerName.includes('permission') ||
      lowerName.includes('auth') ||
      lowerName.includes('login')) {
    return 'admin';
  }
  
  // Default to common
  return 'common';
}

function cleanIndexSignature(content: string): string {
  // Poista [key: string]: any; index signaturet
  // Tämä käsittelee eri muotoja:
  // - [key: string]: any;
  // -    [key: string]: any;
  // - [key: string]: any;
  return content.replace(/\s*\[key:\s*string\]:\s*any;\s*\n?/g, '\n');
}

function extractDependencies(content: string, allTypes: Map<string, TypeDefinition>): string[] {
  const dependencies: string[] = [];
  
  // Look for type references in the content
  for (const [typeName] of allTypes) {
    // Check if this type is referenced in the content
    // Look for typeName as a property type, array type, or union type
    const regex = new RegExp(`\\b${typeName}\\b`, 'g');
    if (regex.test(content) && !content.includes(`export interface ${typeName}`) && !content.includes(`export enum ${typeName}`)) {
      dependencies.push(typeName);
    }
  }
  
  return [...new Set(dependencies)]; // Remove duplicates
}

function findMatchingBrace(content: string, startIndex: number): number {
  let depth = 0;
  let i = startIndex;
  
  while (i < content.length) {
    if (content[i] === '{') {
      depth++;
    } else if (content[i] === '}') {
      depth--;
      if (depth === 0) {
        return i;
      }
    }
    i++;
  }
  
  return -1; // Not found
}

function extractTypes(content: string): TypeDefinition[] {
  const types: TypeDefinition[] = [];
  
  // Find all export interface declarations
  const interfaceRegex = /export\s+interface\s+(\w+)(?:\s*<[^>]*>)?\s*\{/g;
  let match;
  
  while ((match = interfaceRegex.exec(content)) !== null) {
    const name = match[1];
    const startIndex = match.index + match[0].length - 1; // Position of the '{'
    
    const endIndex = findMatchingBrace(content, startIndex);
    if (endIndex === -1) continue;
    
    let fullMatch = content.substring(match.index, endIndex + 1);
    
    // Poista index signaturet
    fullMatch = cleanIndexSignature(fullMatch);
    
    types.push({
      name,
      content: fullMatch,
      category: categorizeType(name),
      dependencies: [] // Will be filled later
    });
  }
  
  // Find all export enum declarations
  const enumRegex = /export\s+enum\s+(\w+)\s*\{/g;
  while ((match = enumRegex.exec(content)) !== null) {
    const name = match[1];
    const startIndex = match.index + match[0].length - 1; // Position of the '{'
    
    const endIndex = findMatchingBrace(content, startIndex);
    if (endIndex === -1) continue;
    
    let fullMatch = content.substring(match.index, endIndex + 1);
    
    // Poista index signaturet (ei pitäisi olla enumeissa, mutta varmuuden vuoksi)
    fullMatch = cleanIndexSignature(fullMatch);
    
    types.push({
      name,
      content: fullMatch,
      category: categorizeType(name),
      dependencies: [] // Will be filled later
    });
  }
  
  // Build a map for dependency extraction
  const typeMap = new Map<string, TypeDefinition>();
  types.forEach(t => typeMap.set(t.name, t));
  
  // Extract dependencies for each type
  types.forEach(type => {
    type.dependencies = extractDependencies(type.content, typeMap);
  });
  
  return types;
}

function generateImports(dependencies: string[], category: string, allTypes: Map<string, TypeDefinition>): string {
  if (dependencies.length === 0) return '';
  
  const imports: string[] = [];
  
  // Group dependencies by their category
  const depsByCategory = new Map<'common' | 'floorball' | 'admin', string[]>();
  
  dependencies.forEach(depName => {
    const depType = allTypes.get(depName);
    if (depType && depType.category !== category) {
      const depCategory = depType.category;
      if (!depsByCategory.has(depCategory)) {
        depsByCategory.set(depCategory, []);
      }
      depsByCategory.get(depCategory)!.push(depName);
    }
  });
  
  // Generate import statements with 'type' keyword
  for (const [depCategory, depNames] of depsByCategory.entries()) {
    const uniqueDeps = [...new Set(depNames)];
    imports.push(`import type { ${uniqueDeps.join(', ')} } from '../${depCategory}/${depCategory}Types.generated';`);
  }
  
  return imports.length > 0 ? imports.join('\n') + '\n' : '';
}

function organizeTypes() {
  console.log('📁 Organizing generated types...');
  
  // Check if generated file exists
  if (!fs.existsSync(GENERATED_FILE)) {
    console.warn(`⚠️  Generated file not found: ${GENERATED_FILE}`);
    console.log('💡 Run type generation first with: npm run generate:types');
    return;
  }
  
  const generatedContent = fs.readFileSync(GENERATED_FILE, 'utf-8');
  
  // If file is empty, exit early
  if (!generatedContent.trim()) {
    console.warn('⚠️  Generated file is empty. Nothing to organize.');
    return;
  }
  
  const types = extractTypes(generatedContent);
  
  if (types.length === 0) {
    console.warn('⚠️  No types found in generated file.');
    return;
  }
  
  console.log(`📋 Found ${types.length} types`);
  
  // Build a map of all types for dependency resolution
  const allTypesMap = new Map<string, TypeDefinition>();
  types.forEach(t => allTypesMap.set(t.name, t));
  
  // Group by category
  const grouped = types.reduce((acc, type) => {
    if (!acc[type.category]) acc[type.category] = [];
    acc[type.category].push(type);
    return acc;
  }, {} as Record<string, TypeDefinition[]>);
  
  // Write to separate files
  for (const [category, categoryTypes] of Object.entries(grouped)) {
    const outputPath = path.join(OUTPUT_DIR, category);
    fs.mkdirSync(outputPath, { recursive: true });
    
    // Generate imports for this category
    const allDependencies = new Set<string>();
    categoryTypes.forEach(type => {
      type.dependencies.forEach(dep => allDependencies.add(dep));
    });
    
    const imports = generateImports([...allDependencies], category, allTypesMap);
    
    // Sort types: enums first, then interfaces
    const sortedTypes = [...categoryTypes].sort((a, b) => {
      const aIsEnum = a.content.includes('export enum');
      const bIsEnum = b.content.includes('export enum');
      if (aIsEnum && !bIsEnum) return -1;
      if (!aIsEnum && bIsEnum) return 1;
      return a.name.localeCompare(b.name);
    });
    
    const content = imports + sortedTypes.map(t => t.content).join('\n\n');
    const filePath = path.join(outputPath, `${category}Types.generated.ts`);
    
    fs.writeFileSync(filePath, content, 'utf-8');
    console.log(`✅ Created ${filePath} with ${categoryTypes.length} types`);
    
    if (allDependencies.size > 0) {
      console.log(`   📦 Imports from other categories: ${[...allDependencies].length}`);
    }
  }
  
  console.log('\n✨ Type organization complete!');
}

// Run the script
organizeTypes();