/*
resetPwdUnvalidatedDangerous
    This function will be run when the client SDK 'callResetPasswordFunction' is called with an object parameter that
    contains five keys: 'token', 'tokenId', 'username', 'password', and 'currentPasswordValid'.
    'currentPasswordValid' is a boolean will be true if a user changes their password by entering their existing
    password and the password matches the actual password that is stored. Additional parameters are passed in as part
    of the argument list from the SDK.

    The return object must contain a 'status' key which can be empty or one of three string values:
      'success', 'pending', or 'fail'

DANGEROUS DEV MODE VERSION fails to validate that the user requesting the reset is correct
*/


  exports = ({ token, tokenId, username, password }) => {
    // will not reset the password
    return { status: 'success' };  // normally 'pending' until callback
  };
