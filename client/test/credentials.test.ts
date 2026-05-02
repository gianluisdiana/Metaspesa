import { describe, expect, it } from 'vitest';

import { Credentials } from '@/lib/auth-domain';

describe('Credentials constructor', () => {
  it('should store the username', () => {
    // Act
    const credentials = new Credentials('alice', 'Pass1word!');

    // Assert
    expect(credentials.username).toBe('alice');
  });

  it('should store the password', () => {
    // Act
    const credentials = new Credentials('alice', 'Pass1word!');

    // Assert
    expect(credentials.password).toBe('Pass1word!');
  });
});

describe('Credentials hasValidUsername', () => {
  it('should return false if username is empty', () => {
    // Arrange
    const credentials = new Credentials('', 'Pass1word!');

    // Act & Assert
    expect(credentials.hasValidUsername()).toBe(false);
  });

  it('should return false if username is only whitespace', () => {
    // Arrange
    const credentials = new Credentials('   ', 'Pass1word!');

    // Act & Assert
    expect(credentials.hasValidUsername()).toBe(false);
  });

  it('should return true if username is non-empty', () => {
    // Arrange
    const credentials = new Credentials('alice', 'Pass1word!');

    // Act & Assert
    expect(credentials.hasValidUsername()).toBe(true);
  });
});

describe('Credentials passwordHasMinimumLength', () => {
  it('should return false if password has fewer than 10 characters', () => {
    // Arrange
    const credentials = new Credentials('alice', 'Short1!');

    // Act & Assert
    expect(credentials.passwordHasMinimumLength()).toBe(false);
  });

  it('should return true if password has exactly 10 characters', () => {
    // Arrange
    const credentials = new Credentials('alice', 'Exactly10!');

    // Act & Assert
    expect(credentials.passwordHasMinimumLength()).toBe(true);
  });

  it('should return true if password has more than 10 characters', () => {
    // Arrange
    const credentials = new Credentials('alice', 'LongPassword1!');

    // Act & Assert
    expect(credentials.passwordHasMinimumLength()).toBe(true);
  });
});

describe('Credentials passwordHasUppercase', () => {
  it('should return false if password has no uppercase letter', () => {
    // Arrange
    const credentials = new Credentials('alice', 'nouppercase1!');

    // Act & Assert
    expect(credentials.passwordHasUppercase()).toBe(false);
  });

  it('should return true if password has at least one uppercase letter', () => {
    // Arrange
    const credentials = new Credentials('alice', 'HasUppercase1!');

    // Act & Assert
    expect(credentials.passwordHasUppercase()).toBe(true);
  });
});

describe('Credentials passwordHasLowercase', () => {
  it('should return false if password has no lowercase letter', () => {
    // Arrange
    const credentials = new Credentials('alice', 'NOLOWERCASE1!');

    // Act & Assert
    expect(credentials.passwordHasLowercase()).toBe(false);
  });

  it('should return true if password has at least one lowercase letter', () => {
    // Arrange
    const credentials = new Credentials('alice', 'HasLowercase1!');

    // Act & Assert
    expect(credentials.passwordHasLowercase()).toBe(true);
  });
});

describe('Credentials passwordHasDigit', () => {
  it('should return false if password has no digit', () => {
    // Arrange
    const credentials = new Credentials('alice', 'NoDigitHere!');

    // Act & Assert
    expect(credentials.passwordHasDigit()).toBe(false);
  });

  it('should return true if password has at least one digit', () => {
    // Arrange
    const credentials = new Credentials('alice', 'HasDigit1!');

    // Act & Assert
    expect(credentials.passwordHasDigit()).toBe(true);
  });
});

describe('Credentials passwordHasSpecialCharacter', () => {
  it('should return false if password has no special character', () => {
    // Arrange
    const credentials = new Credentials('alice', 'NoSpecialChar1');

    // Act & Assert
    expect(credentials.passwordHasSpecialCharacter()).toBe(false);
  });

  it('should return true if password has at least one special character', () => {
    // Arrange
    const credentials = new Credentials('alice', 'HasSpecial1!');

    // Act & Assert
    expect(credentials.passwordHasSpecialCharacter()).toBe(true);
  });
});

describe('Credentials hasValidPassword', () => {
  it('should return false if password is too short', () => {
    // Arrange
    const credentials = new Credentials('alice', 'Sh0rt!');

    // Act & Assert
    expect(credentials.hasValidPassword()).toBe(false);
  });

  it('should return false if password has no uppercase letter', () => {
    // Arrange
    const credentials = new Credentials('alice', 'nouppercase1!');

    // Act & Assert
    expect(credentials.hasValidPassword()).toBe(false);
  });

  it('should return false if password has no lowercase letter', () => {
    // Arrange
    const credentials = new Credentials('alice', 'NOLOWERCASE1!');

    // Act & Assert
    expect(credentials.hasValidPassword()).toBe(false);
  });

  it('should return false if password has no digit', () => {
    // Arrange
    const credentials = new Credentials('alice', 'NoDigitHere!');

    // Act & Assert
    expect(credentials.hasValidPassword()).toBe(false);
  });

  it('should return false if password has no special character', () => {
    // Arrange
    const credentials = new Credentials('alice', 'NoSpecialChar1');

    // Act & Assert
    expect(credentials.hasValidPassword()).toBe(false);
  });

  it('should return true if password meets all requirements', () => {
    // Arrange
    const credentials = new Credentials('alice', 'ValidPass1!');

    // Act & Assert
    expect(credentials.hasValidPassword()).toBe(true);
  });
});

describe('Credentials isValid', () => {
  it('should return false if username is invalid', () => {
    // Arrange
    const credentials = new Credentials('', 'ValidPass1!');

    // Act & Assert
    expect(credentials.isValid()).toBe(false);
  });

  it('should return false if password is invalid', () => {
    // Arrange
    const credentials = new Credentials('alice', 'weak');

    // Act & Assert
    expect(credentials.isValid()).toBe(false);
  });

  it('should return true if both username and password are valid', () => {
    // Arrange
    const credentials = new Credentials('alice', 'ValidPass1!');

    // Act & Assert
    expect(credentials.isValid()).toBe(true);
  });
});
